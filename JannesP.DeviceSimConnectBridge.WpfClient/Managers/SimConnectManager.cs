using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JannesP.SimConnectWrapper;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Managers
{
    public class SimConnectManager : IDisposable
    {
        public SimConnectManager(ILogger<SimConnectManager> logger, ApplicationOptions options)
        {
            _logger = logger;
            _options = options;
            _options.PropertyChanged += OnOptions_PropertyChanged;
        }

        public event EventHandler<StateChangedEventArgs>? StateChanged;
        public event EventHandler? ConnectLoopError;

        public State ConnectionState { get; private set; } = State.Disconnected;
        public SimConnectWrapper.SimConnectWrapper? SimConnectWrapper => ConnectionState == State.Connected ? _simConnect : null;

        private readonly SemaphoreSlim _semState = new(1);
        private readonly ILogger<SimConnectManager> _logger;
        private readonly ApplicationOptions _options;
        private CancellationTokenSource? _ctsConnect;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "We need to keep a reference to the task, otherwise it'll just get collected.")]
        private Task? _taskConnect;
        private SimConnectWrapper.SimConnectWrapper? _simConnect;

        private async Task<bool> TransitionStateAsync(State currentState, State newState)
        {
            await _semState.WaitAsync().ConfigureAwait(false);
            try
            {
                if (currentState != ConnectionState)
                {
                    return false;
                }
                ConnectionState = newState;
                return true;
            }
            finally
            {
                _semState.Release();
            }
        }

        private async Task<State> TransitionStateAsync(State newState)
        {
            await _semState.WaitAsync().ConfigureAwait(false);
            State oldState;
            try
            {
                oldState = ConnectionState;
                ConnectionState = newState;
                return oldState;
            }
            finally
            {
                _semState.Release();
            }
            throw new Exception("Error transitioning state.");
        }

        private void OnStateTransition(State oldState, State newState)
        {
            StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, newState));
        }
        


        private void OnOptions_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ApplicationOptions.SimConnectApplicationName):
                    throw new NotSupportedException($"Cannot change {e.PropertyName} during runtime.");
            }
        }

        public enum State
        {
            Disconnected,
            Connecting,
            ConnectingWaitingForResponse,
            Connected,
            Disconnecting,
        }

        public async Task StartAsync()
        {
            _logger.LogInformation("Starting SimConnectManager!");
            await StartConnectLoopAsync().ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping SimConnectManager ...");
            State oldState = await TransitionStateAsync(State.Disconnecting);
            OnStateTransition(oldState, State.Disconnecting);
            try
            {
                if (_ctsConnect != null)
                {
                    _ctsConnect.Cancel();
                    _ctsConnect.Dispose();
                    _ctsConnect = null;
                    _taskConnect = null;
                    if (_simConnect != null)
                    {
                        try
                        {
                            await _simConnect.Disconnect().ConfigureAwait(false);
                        }
                        catch { }
                        _simConnect.Dispose();
                        _simConnect = null;
                    }
                }
            }
            finally
            {
                await TransitionStateAsync(State.Disconnected);
                OnStateTransition(State.Disconnecting, State.Disconnected);
            }
            _logger.LogTrace("Stopped SimConnectManager!");
        }

        public void Dispose()
        {
            TransitionStateAsync(State.Disconnecting).Wait();
            _ctsConnect?.Cancel();
            _ctsConnect?.Dispose();
            _ctsConnect = null;
            _taskConnect = null;
            _simConnect?.Dispose();
            _simConnect = null;
            TransitionStateAsync(State.Disconnected).Wait();
        }

        private async Task StartConnectLoopAsync()
        {
            if (await TransitionStateAsync(State.Disconnected, State.Connecting))
            {
                OnStateTransition(State.Disconnected, State.Connecting);
                _ctsConnect = new CancellationTokenSource();
                _taskConnect = ConnectLoopAsync(_ctsConnect.Token);
            }
        }

        private async Task ConnectLoopAsync(CancellationToken ct)
        {
            _logger.LogInformation("ConnectLoop started!");
            try
            {
                while (!ct.IsCancellationRequested && ConnectionState == State.Connecting)
                {
                    try
                    {
                        if (await ConnectAsync().ConfigureAwait(false))
                        {
                            _logger.LogTrace("SimConnect Connect() == true");                            
                            await TransitionStateAsync(State.Connecting, State.ConnectingWaitingForResponse);
                            break;
                        }
                        else
                        {
                            int delay = _options.SimConnectConnectRetryDelay;
                            _logger.LogTrace("Failed to connect to SimConnect, retrying in {0}", delay);
                            await Task.Delay(delay, ct).ConfigureAwait(false);
                        }
                    }
                    catch (TaskCanceledException) 
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connect loop error!");
                ConnectLoopError?.Invoke(this, new EventArgs());
            }
            finally
            {
                _ctsConnect?.Dispose();
                _ctsConnect = null;
                _taskConnect = null;
            }
            _logger.LogInformation("ConnectLoop ended!");
        }

        private async Task<bool> ConnectAsync()
        {
            if (_simConnect == null)
            {
                _simConnect = new SimConnectWrapper.SimConnectWrapper(_options.SimConnectApplicationName);
                _simConnect.SimConnectOpen += OnSimConnect_SimConnectOpen;
                _simConnect.SimConnectClose += OnSimConnect_SimConnectClose;
            }
            return await _simConnect.TryConnect();
        }

        private async void OnSimConnect_SimConnectClose(object? sender, EventArgs e)
        {
            _logger.LogInformation("SimConnect disconnected.");
            if (ConnectionState == State.Connected)
            {
                _logger.LogInformation("State was '{0}', recycling for reconnect ...", nameof(State.Connected));
                await StopAsync();
                await StartAsync();
                _logger.LogInformation("Recycle complete.");
            }
        }

        private async void OnSimConnect_SimConnectOpen(object? sender, EventArgs e)
        {
            _logger.LogInformation("SimConnect connected.");
            if (await TransitionStateAsync(State.ConnectingWaitingForResponse, State.Connected))
            {
                OnStateTransition(State.ConnectingWaitingForResponse, State.Connected);
            }
        }

        public class StateChangedEventArgs : EventArgs
        {
            public State NewState { get; }
            public State OldState { get; }

            public StateChangedEventArgs(State oldState, State newState)
            {
                OldState = oldState;
                NewState = newState;
            }
        }
    }
}
