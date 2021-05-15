using System;
using System.Windows;
using JannesP.DeviceSimConnectBridge.WpfApp.View.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Extensions
{
    internal static class ApplicationExtensions
    {
        /// <summary>
        /// Shows the main window or creates it.
        /// </summary>
        /// <param name="app">the application for which to open the window</param>
        /// <param name="createdNew">If a new window was created or if the MainWindow already had a window.</param>
        /// <returns></returns>
        public static MainWindow ShowCreateMainWindow(this App app, out bool createdNew)
        {
            createdNew = app.MainWindow == null;
            if (createdNew)
            {
                MainWindow window = app.Host.Services.GetRequiredService<MainWindow>();
                window.Show();
                Application.Current.MainWindow = window;
                return window;
            }
            else
            {
                if (app.MainWindow is MainWindow currentWindow)
                {
                    if (currentWindow.WindowState == WindowState.Minimized) currentWindow.WindowState = WindowState.Normal;
                    currentWindow.Activate();
                    return currentWindow;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"The current window is not of the type {typeof(MainWindow).FullName}");
                }
            }
        }

        /// <summary>
        /// Shows the main window or creates it.
        /// </summary>
        /// <returns></returns>
        public static MainWindow ShowCreateMainWindow(this App app) => app.ShowCreateMainWindow(out _);
    }
}