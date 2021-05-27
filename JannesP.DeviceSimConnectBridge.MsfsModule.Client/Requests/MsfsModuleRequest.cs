using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.MsfsModule.Client.Requests
{
    public abstract class MsfsModuleRequest
    {
        private static readonly Random _random = new Random();
        public static string DataAreaModuleBaseName { get; } = "jannesp.device_simconnect_bridge";
        public static string DataAreaModuleInputBaseName { get; } = $"{DataAreaModuleBaseName}.module_input";
        public static string DataAreaModuleOutputBaseName { get; } = $"{DataAreaModuleBaseName}.module_output";

        protected int NextRequestId
        {
            get
            {
                lock (_random) { return _random.Next(1, int.MaxValue); }
            }
        }
    }

    public abstract class MsfsModuleRequestWithResponse : MsfsModuleRequest
    {
        internal abstract void Finish(object response);
    }

    public abstract class MsfsModuleRequestWithResponse<TRes> : MsfsModuleRequestWithResponse, IDisposable
    {
        private static readonly Dictionary<int, MsfsModuleRequestWithResponse<TRes>> _pendingRequests = new Dictionary<int, MsfsModuleRequestWithResponse<TRes>>();

        private readonly CancellationTokenSource _cts;
        private readonly TaskCompletionSource<TRes> _taskCompletionSource;
        private bool _disposedValue = false;

        protected MsfsModuleRequestWithResponse()
        {
            MyRequestId = NextRequestId;
            _taskCompletionSource = new TaskCompletionSource<TRes>();
            _cts = new CancellationTokenSource();
            _cts.Token.Register(() =>
            {
                lock (_pendingRequests)
                {
                    _pendingRequests.Remove(MyRequestId);
                }
                lock (_taskCompletionSource)
                {
                    _taskCompletionSource.TrySetException(new TimeoutException("The request didnt get a valid response in time."));
                }
            });
        }

        protected int MyRequestId { get; set; }

        protected static void FinishWithId(int requestId, TRes result)
        {
            lock (_pendingRequests)
            {
                if (_pendingRequests.TryGetValue(requestId, out MsfsModuleRequestWithResponse<TRes> request))
                {
                    _pendingRequests.Remove(requestId);
                    request.CompleteTask(result);
                }
            }
        }

        protected void CompleteTask(TRes result)
        {
            _taskCompletionSource.TrySetResult(result);
        }

        protected Task<TRes> WaitForFinish(TimeSpan timeout)
        {
            lock (_pendingRequests)
            {
                _pendingRequests.Add(MyRequestId, this);
            }
            _cts.CancelAfter(timeout);
            return _taskCompletionSource.Task;
        }

        #region Dispose

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                }
                _disposedValue = true;
            }
        }

        #endregion Dispose
    }
}