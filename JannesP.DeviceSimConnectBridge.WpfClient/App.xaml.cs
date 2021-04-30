using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.Device.XTouchMini;
using JannesP.DeviceSimConnectBridge.WpfApp.Extensions;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.Resources;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility;
using JannesP.DeviceSimConnectBridge.WpfApp.View;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IHost Host { get; }
        private readonly Lazy<ILogger<App>> _logger;

        public App()
        {
            Host = new HostBuilder()
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder.SetBasePath(AssemblyUtil.AssemblyDirectory);
                    configBuilder.AddJsonFile("appsettings.json", true);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddDebug();
                })
                .ConfigureServices((context, services) =>
                {
                    //configuration
                    services.Configure<ApplicationConfiguration>(context.Configuration);
                    services.AddSingleton<ApplicationOptionsManager>();
                    services.AddTransient<ApplicationOptions>(provider => provider.GetRequiredService<ApplicationOptionsManager>().Options);

                    //basic application lifecycle services
                    services.AddSingleton<TrayIconManager>(provider => new TrayIconManager(ResourcePaths.TrayIcon));
                    services.AddSingleton<SingleInstanceManager>();

                    //repositories
                    services.AddSingleton<ProfileRepository>();
                    services.AddSingleton<DeviceRepository>();

                    //managers
                    services.AddSingleton<ProfileManager>();
                    services.AddSingleton<SimConnectManager>();
                    services.AddSingleton<DeviceBindingManager>();
                    
                    //viewmodels
                    services.AddTransient<SimConnectManagerViewModel>(provider => new SimConnectManagerViewModel(provider.GetRequiredService<SimConnectManager>()));
                    services.AddTransient<MainWindowViewModel>(provider => new MainWindowViewModel(provider.GetRequiredService<SimConnectManagerViewModel>()));
                    
                    //views
                    services.AddTransient<MainWindow>();
                })
                .Build();
            _logger = new Lazy<ILogger<App>>(() => Host.Services.GetRequiredService<ILogger<App>>());
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            await Host.StartAsync();

            SingleInstanceManager singleInstanceManager = Host.Services.GetRequiredService<SingleInstanceManager>();

            _logger.Value.LogInformation("Starting App.");

            if (!singleInstanceManager.IsFirstInstance)
            {
                _logger.Value.LogInformation("I am not the first instance. Shutting down ...");
                base.Shutdown(0);
                return;
            }

            base.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            singleInstanceManager.SecondInstanceStarted += OnSecondInstanceStarted;

            var createUi = true;
            foreach (string arg in e.Args)
            {
                switch (arg)
                {
                    case "-tray":
                        createUi = false;
                        _logger.Value.LogInformation("Launch argument '{0}' found, don't create a UI.", arg);
                        break;
                    default:
                        _logger.Value.LogWarning("Got invalid launch argument '{0}', ignoring.", arg);
                        break;
                }
            }

            _logger.Value.LogTrace("Setting up tray icon.");
            TrayIconManager trayIconManager = Host.Services.GetRequiredService<TrayIconManager>();
            trayIconManager.IconVisible = true;
            trayIconManager.ItemExitClick += (sender, eventArgs) => base.Shutdown(0);
            trayIconManager.DoubleClick += (sender, eventArgs) => this.ShowCreateMainWindow();

            if (createUi)
            {
                _logger.Value.LogInformation("Creating MainWindow on startup.");
                this.ShowCreateMainWindow();
            }
            var scm = Host.Services.GetRequiredService<SimConnectManager>();
            await scm.StartAsync().ConfigureAwait(false);
            var dbm = Host.Services.GetRequiredService<DeviceBindingManager>();
            await dbm.Enable().ConfigureAwait(false);
        }

        private void OnSecondInstanceStarted(object? sender, EventArgs e)
        {
            _logger.Value.LogInformation("Second instance started, showing MainWindow.");
            base.Dispatcher.Invoke(this.ShowCreateMainWindow);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Value.LogCritical(e.Exception, "Error caught in App_DispatcherUnhandledException, crashing ...");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            _logger.Value.LogCritical(ex, "Error caught in CurrentDomain_UnhandledException, crashing ...");
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);
            base.Shutdown(0);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Current.MainWindow?.Close();
            var repo = Host.Services.GetRequiredService<ProfileRepository>();
            repo.PersistProfilesAsync().Wait();
            using (Host)
            {
                Host.StopAsync(TimeSpan.FromSeconds(2)).Wait();
            }
        }
    }
}
