using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Managers
{
    internal class SingleInstanceManager : IDisposable
    {
        private readonly string _syncNamesBase;
        private string MutexName => _syncNamesBase + "_isFirstInstanceMutex";
        private string EventSignalName => _syncNamesBase + "_notifyFirstInstanceEvent";
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly EventWaitHandle? _evtNotifiedFromOtherProcess;
        private readonly Mutex _mutexCheckIfFirstInstance;
        private readonly ILogger<SingleInstanceManager> _logger;

        /// <summary>
        /// Creates a new Instance that monitors for another process with the same WaitHandles and Mutexes.
        /// </summary>
        /// <param name="systemUniqueIdentifierName">System-wide unique name to identify this application.</param>
        /// <param name="notifyIfNotFirst">if the other application should be notified that we checked for the instance</param>
        /// <exception cref="ArgumentException">if the systemUniqueIdentifierName is an empty string</exception>
        /// <exception cref="IOException">if an error happens while initializing any of the system allocations</exception>
        public SingleInstanceManager(ILogger<SingleInstanceManager> logger, IOptions<ApplicationOptions> config)
        {
            _logger = logger;
            _syncNamesBase = config.Value.SingleInstanceMutexName;
            if (string.IsNullOrWhiteSpace(config.Value.SingleInstanceMutexName))
            {
                throw new ConfigurationErrorsException("ApplicationOptions.SingleInstanceMutexName must have a value");
            }
            _mutexCheckIfFirstInstance = new Mutex(false, MutexName);
            try
            {
                IsFirstInstance = _mutexCheckIfFirstInstance.WaitOne(0, false);
            }
            catch (AbandonedMutexException)
            {
                /* ignored because we got it and it doesn't protect any data */
                IsFirstInstance = true;
            }
            if (!IsFirstInstance && config.Value.SingleInstanceNotifyFirstInstance)
            {
                try
                {
                    _evtNotifiedFromOtherProcess = EventWaitHandle.OpenExisting(EventSignalName);
                    _evtNotifiedFromOtherProcess.Set();
                }
                catch (Exception)
                { //ignored
                }
            }
            else if (IsFirstInstance)
            {
                try
                {
                    _evtNotifiedFromOtherProcess = new EventWaitHandle(false, EventResetMode.ManualReset,
                        EventSignalName, out bool createdNew);
                    if (createdNew)
                    {
                        Task.Run(() => WaitForEvent(_evtNotifiedFromOtherProcess, _cancellationTokenSource.Token));
                    }
                }
                catch (Exception ex)
                {
                    throw new IOException("Error creating event for listening to notify SecondInstanceStarted. See the InnerException for more details.", ex);
                }
            }
        }

        /// <summary>
        /// Gets called when this process is notified by another process.
        /// </summary>
        public event EventHandler? SecondInstanceStarted;
        /// <summary>
        /// If the current instance is the first instance.
        /// </summary>
        public bool IsFirstInstance { get; }

        private void WaitForEvent(EventWaitHandle ewh, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                ewh.WaitOne();
                if (!token.IsCancellationRequested)
                {
                    OnSecondInstanceStarted();
                    ewh.Reset();
                }
            }
        }

        private void OnSecondInstanceStarted()
        {
            _logger.LogInformation("Second instance started.");
            SecondInstanceStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Disposes of all used native resources.
        /// </summary>
        public void Dispose()
        {
            _evtNotifiedFromOtherProcess?.Dispose();
            if (IsFirstInstance)
            {
                try
                {
                    _mutexCheckIfFirstInstance.ReleaseMutex();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error releasing _mutexCheckIfFirstInstance.");
                }
            }
            _mutexCheckIfFirstInstance.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
