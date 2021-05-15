using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Utility
{
    internal static class AutostartHelper
    {
        private static readonly string _registryAutostartKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly string _autostartKeyName = AssemblyUtil.AssemblyNameWithoutExtension;

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
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(_registryAutostartKey, true))
                {
                    if (key != null)
                    {
                        key.SetValue(_autostartKeyName, $"\"{AssemblyUtil.FullAssemblyPath}\"{args}");
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
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(_registryAutostartKey, true))
                {
                    if (key != null)
                    {
                        if (key.GetValue(_autostartKeyName) != null)
                        {
                            key.DeleteValue(_autostartKeyName);
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
