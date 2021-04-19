using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace JannesP.XTouchMiniSimConnectBridge.WpfApp.Utility
{
    internal static class AutostartHelper
    {
        private static readonly string RegistryAutostartKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly string AutostartKeyName = AssemblyUtil.AssemblyNameWithoutExtension;

        /// <summary>
        /// Restarts the current executable. You can use this if you used the WpfUtil Mutex to handle multiple instances.
        /// </summary>
        public static void Restart()
        {
            Application.Current.Exit += (o, args) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = AssemblyUtil.FullAssemblyPath,
                    WorkingDirectory = Process.GetCurrentProcess().StartInfo.WorkingDirectory
                });
            };
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Adds the program to the autostart in the registry. You should probably use some command line args to prevent the gui from opening.
        /// If the regkeyname is not set it will use the executable name without the extension.
        /// </summary>
        /// <param name="additionalArgs">additional command line args to be used</param>
        /// <returns>if the key was set successfully</returns>
        public static bool AddToAutostart(string? additionalArgs = null)
        {
            string args;
            if (additionalArgs == null)
            {
                args = "";
            }
            else
            {
                args = " " + additionalArgs;
            }

            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryAutostartKey, true))
                {
                    if (key != null)
                    {
                        key.SetValue(AutostartKeyName, $"\"{AssemblyUtil.FullAssemblyPath}\"{args}");
                        return true;
                    }
                    else
                    {
                        Trace.TraceError("Autostart key couldn't be opened!");
                    }
                }
            }
            catch (SecurityException ex)
            {
                Trace.TraceError("Error AddToAutostart:\n{0}", ex.GetType().Name + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Removes the program from autostart, deleting the registry key. It will use the last key name that was used to set in this instance.
        /// </summary>
        /// <returns>if the key was removed successfully</returns>
        public static bool RemoveFromAutostart()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryAutostartKey, true))
                {
                    if (key != null)
                    {
                        if (key.GetValue(AutostartKeyName) != null)
                        {
                            key.DeleteValue(AutostartKeyName);
                        }
                        return true;
                    }
                    else
                    {
                        Trace.TraceError("Autostart key couldn't be opened!");
                    }
                }
            }
            catch (SecurityException ex)
            {
                Trace.TraceError("Error RemoveFromAutostart:\n{0}", ex.GetType().Name + ex.Message);
            }
            return false;
        }
    }
}
