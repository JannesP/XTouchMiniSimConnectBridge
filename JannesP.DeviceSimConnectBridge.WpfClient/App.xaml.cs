using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using JannesP.DeviceSimConnectBridge.WpfApp.Extensions;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.Resources;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility;
using JannesP.DeviceSimConnectBridge.WpfApp.View.Windows;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels;
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
        private readonly Lazy<ILogger<App>> _logger;
        private int _isTeardown = 0;

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
                    services.AddSingleton<BindingActionRepository>();

                    //managers
                    services.AddSingleton<ProfileManager>();
                    services.AddSingleton<SimConnectManager>();
                    services.AddSingleton<DeviceBindingManager>();

                    //viewmodels
                    services.AddTransient<ProfileManagementViewModel>();
                    services.AddTransient<SimConnectManagerViewModel>();
                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<ProfileManagementWindowViewModel>();

                    //views
                    services.AddTransient<ProfileManagementWindow>();
                    services.AddTransient<MainWindow>();
                })
                .Build();
            _logger = new Lazy<ILogger<App>>(() => Host.Services.GetRequiredService<ILogger<App>>());
        }

        public IHost Host { get; }

        public static async Task GracefulShutdownAsync()
        {
            await Teardown((App)Current, false);
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Teardown(this, true).Wait();
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);
            base.Shutdown();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

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

            bool createUi = true;
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
            trayIconManager.ItemExitClick += (sender, eventArgs) => _ = App.GracefulShutdownAsync();
            trayIconManager.DoubleClick += (sender, eventArgs) => this.ShowCreateMainWindow();

            ProfileRepository profileRepository = Host.Services.GetRequiredService<ProfileRepository>();
            await profileRepository.LoadProfilesAsync();

            if (createUi)
            {
                _logger.Value.LogInformation("Creating MainWindow on startup.");
                this.ShowCreateMainWindow();
            }

            SimConnectManager scm = Host.Services.GetRequiredService<SimConnectManager>();
            await scm.StartAsync();
            _ = Host.Services.GetRequiredService<DeviceBindingManager>();
        }

        private static async Task Teardown(App app, bool fastTeardown)
        {
            if (Interlocked.Exchange(ref app._isTeardown, 1) == 0)
            {
                app.Dispatcher.Invoke(() => Current.MainWindow?.Close());
                if (!fastTeardown)
                {
                    ProfileRepository? repo = app.Host.Services.GetRequiredService<ProfileRepository>();
                    await repo.PersistProfilesAsync().ConfigureAwait(false);
                }
                using (app.Host)
                {
                    await app.Host.StopAsync();
                }
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
            => _logger.Value.LogCritical(e.Exception, "Error caught in App_DispatcherUnhandledException, crashing ...");

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            _logger.Value.LogCritical(ex, "Error caught in CurrentDomain_UnhandledException, crashing ...");
        }

        private void OnSecondInstanceStarted(object? sender, EventArgs e)
        {
            _logger.Value.LogInformation("Second instance started, showing MainWindow.");
            base.Dispatcher.Invoke(this.ShowCreateMainWindow);
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.Value.LogError(e.Exception, "Unobserved exception in task.");
            e.SetObserved();
        }
    }
}