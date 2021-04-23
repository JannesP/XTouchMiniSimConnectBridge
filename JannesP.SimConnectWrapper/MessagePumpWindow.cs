using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static JannesP.SimConnectWrapper.Win32;

namespace JannesP.SimConnectWrapper
{
    internal class MessagePumpWindow : IDisposable
    {
        public IntPtr Handle { get; private set; }

        public event EventHandler? MessagePumpDestroyed;

        private static readonly object _syncRoot = new object();
        private static Dictionary<IntPtr, MessagePumpWindow> _windows = new Dictionary<IntPtr, MessagePumpWindow>();
        private static WNDCLASSEX _wndClass = new WNDCLASSEX();
        private static ushort? _wndClassResult;
        private static WndProc? _wndProcStatic;

        private readonly Thread _thread;
        private TaskCompletionSource<bool>? _createTaskCompletionSource;
        private WndProc _wndProc;
        private bool _disposedValue;
        

        public MessagePumpWindow(WndProc wndProc)
        {
            _wndProc = wndProc;
            lock (_syncRoot)
            {
                
                _thread = new Thread(MessagePump);
                RegisterWindowClass();
            }
        }

        public Task Create()
        {
            if (_createTaskCompletionSource == null)
            {
                _createTaskCompletionSource = new TaskCompletionSource<bool>();
                _thread.Start();
            }
            return _createTaskCompletionSource.Task;
        }

        private void MessagePump()
        {
            try
            {
                if (_wndClassResult == null) throw new Exception("Woops, this isn't supposed to happen :(");
                Console.WriteLine("Window Thread start!");
                Handle = Win32.CreateWindowEx(0, new IntPtr((int)(uint)_wndClassResult.Value), "XTouchMiniSimconnectBridgeMessagePump", 0, 0, 0, 0, 0, Win32.HWND_MESSAGE, IntPtr.Zero, _wndClass.hInstance, IntPtr.Zero);
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                Console.WriteLine($"Handle: {Handle:X16}");
                _windows.Add(Handle, this);
            }
            catch (Exception ex)
            {
                _createTaskCompletionSource?.SetException(ex);
                throw;
            }
            _createTaskCompletionSource?.SetResult(true);
            MSG msg;
            sbyte getMsgResult;
            while ((getMsgResult = GetMessage(out msg, IntPtr.Zero, 0, 0)) != 0)
            {
                if (getMsgResult == -1)
                {
                    break;
                }
                else
                {
                    //TranslateMessage(ref msg);
                    try
                    {
                        DispatchMessage(ref msg);
                    }
#pragma warning disable CS0618 // Type or member is obsolete -- well, it does raise it Sadge
                    catch (ExecutionEngineException)
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        break;
                    }
                }
            }
            MessagePumpDestroyed?.Invoke(this, new System.EventArgs());
            Console.WriteLine("Window Thread exit!");
        }

        private static void RegisterWindowClass()
        {
            if (!_wndClassResult.HasValue)
            {
                _wndClass.cbSize = Marshal.SizeOf<WNDCLASSEX>();
                _wndClass.hInstance = Marshal.GetHINSTANCE(typeof(MessagePumpWindow).Module);
                _wndClass.lpszClassName = "XTouchMiniSimconnectBridgeMessagePump";
                _wndProcStatic = new WndProc(WndProcStatic);
                _wndClass.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcStatic);
                _wndClassResult = Win32.RegisterClassEx(ref _wndClass);
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
        }

        private static IntPtr WndProcStatic(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WindowMessage.WM_CLOSE || msg == WindowMessage.WM_DESTROY)
            {
                PostQuitMessage(0);
                return IntPtr.Zero;
            }
            if (_windows.TryGetValue(hWnd, out MessagePumpWindow window))
            {
                return window._wndProc.Invoke(hWnd, msg, wParam, lParam);
            }
            else
            {
                return DefWindowProc(hWnd, msg, wParam, lParam);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                lock (_syncRoot) 
                {
                    if (Handle != null)
                    {
                        SendMessage(Handle, WindowMessage.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        _windows.Remove(Handle);
                    }
                }
                _disposedValue = true;
            }
        }

        ~MessagePumpWindow()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
