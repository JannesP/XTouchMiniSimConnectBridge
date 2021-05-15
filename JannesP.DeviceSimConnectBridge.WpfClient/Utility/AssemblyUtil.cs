using System;
using System.IO;
using System.Reflection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Utility
{
    internal static class AssemblyUtil
    {
        public static string AssemblyDirectory => Path.GetDirectoryName(FullAssemblyPath) ?? throw new Exception("Error retrieving the application directory.");

        public static string AssemblyName => Path.GetFileName(FullAssemblyPath);

        public static string AssemblyNameWithoutExtension => Path.GetFileNameWithoutExtension(FullAssemblyPath);

        /// <summary>
        /// The .net Framework version didn't handle all paths well, so I precautiously put this in a util to fix potential bugs again.
        /// For example every path with a '#' in a folder name just broke.
        /// </summary>
        public static string FullAssemblyPath => Assembly.GetEntryAssembly()?.Location ?? throw new Exception("Couldn't get info about EntryAssembly.");
    }
}