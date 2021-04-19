using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using JannesP.XTouchMiniSimConnectBridge.WpfApp.Extensions;
using JannesP.XTouchMiniSimConnectBridge.WpfApp.Logging;
using JannesP.XTouchMiniSimConnectBridge.WpfApp.Resources;
using JannesP.XTouchMiniSimConnectBridge.WpfApp.Utility;

namespace JannesP.XTouchMiniSimConnectBridge.WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TrayIconManager? _trayIconManager;
        private SingleInstanceManager? _singleInstanceManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ConsoleEventListener? _ = ConsoleEventListener.Instance;
            ApplicationEventSource.Log.Startup();

            _singleInstanceManager = new SingleInstanceManager("JannesP_XTouchMiniSimConnectBridge");

            if (!_singleInstanceManager.IsFirstInstance)
            {
                ApplicationEventSource.Log.NotFirstInstance();
                base.Shutdown(0);
                return;
            }

            base.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _singleInstanceManager.SecondInstanceStarted += OnSecondInstanceStarted;

            var createUi = true;
            foreach (string arg in e.Args)
            {
                switch (arg)
                {
                    case "-tray":
                        createUi = false;
                        break;
                    default:
                        ApplicationEventSource.Log.Warning($"Got invalid launch argument '{arg}', ignoring ...");
                        break;
                }
            }
            _trayIconManager = new TrayIconManager(ResourcePaths.TrayIcon) { IconVisible = true };
            _trayIconManager.ItemExitClick += (sender, eventArgs) => base.Shutdown(0);
            _trayIconManager.DoubleClick += (sender, eventArgs) => this.ShowCreateMainWindow<MainWindow>();
            if (createUi)
            {
                this.ShowCreateMainWindow<MainWindow>();
            }
        }

        private void OnSecondInstanceStarted(object? sender, EventArgs e)
        {
            ApplicationEventSource.Log.SecondInstanceStarted();
            base.Dispatcher.Invoke(this.ShowCreateMainWindow<MainWindow>);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ApplicationEventSource.Log.FatalException(e.Exception);
            Cleanup();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            ApplicationEventSource.Log.FatalException(ex);
            Cleanup();
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);
            base.Shutdown(0);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Cleanup();
        }

        private void Cleanup()
        {
            _trayIconManager?.Dispose();
            _singleInstanceManager?.Dispose();
            ConsoleEventListener.Instance.Dispose();
            ApplicationEventSource.Log.Dispose();
        }
    }
}
