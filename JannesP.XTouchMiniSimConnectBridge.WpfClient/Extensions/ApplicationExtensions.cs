using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JannesP.XTouchMiniSimConnectBridge.WpfApp.Extensions
{
    internal static class ApplicationExtensions
    {
        /// <summary>
        /// Shows the main window or creates it with the given Type.
        /// </summary>
        /// <typeparam name="TWindow">The type for window to use. Throws an exception if the current Window exists and doesnt have correct type.</typeparam>
        /// <param name="app">the application for which to open the window</param>
        /// <param name="createdNew">If a new window was created or if the MainWindow already had a window.</param>
        /// <returns></returns>
        public static TWindow ShowCreateMainWindow<TWindow>(this Application app, out bool createdNew) where TWindow : Window
        {
            createdNew = app.MainWindow == null;
            if (createdNew)
            {
                TWindow window = Activator.CreateInstance<TWindow>();
                window.Show();
                Application.Current.MainWindow = window;
                return window;
            }
            else
            {
                if (app.MainWindow is TWindow currentWindow)
                {
                    if (currentWindow.WindowState == WindowState.Minimized) currentWindow.WindowState = WindowState.Normal;
                    currentWindow.Activate();
                    return currentWindow;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"The current window is not of the type {typeof(TWindow).FullName}");
                }
            }
        }

        /// <summary>
        /// Shows the main window or creates it with the given Type.
        /// </summary>
        /// <typeparam name="TWindow">The type for window to use. Throws an exception if the current Window exists and doesnt have correct type.</typeparam>
        /// <returns></returns>
        public static TWindow ShowCreateMainWindow<TWindow>(this Application app) where TWindow : Window
        {
            return app.ShowCreateMainWindow<TWindow>(out _);
        }
    }
}
