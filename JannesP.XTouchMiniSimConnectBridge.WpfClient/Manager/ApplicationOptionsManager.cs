using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.XTouchMiniSimConnectBridge.WpfApp.Options;
using JannesP.XTouchMiniSimConnectBridge.WpfApp.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JannesP.XTouchMiniSimConnectBridge.WpfApp.Manager
{
    internal class ApplicationOptionsManager
    {
        private readonly ILogger<ApplicationOptionsManager> _logger;

        public ApplicationOptions Options { get; }

        public ApplicationOptionsManager(ILogger<ApplicationOptionsManager> logger)
        {
            _logger = logger;
            var file = new FileInfo(Path.Combine(AssemblyUtil.AssemblyDirectory, "ApplicationOptions.json"));
            if (file.Exists)
            {
                try
                {
                    string optionsText = File.ReadAllText(file.FullName);
                    Options = JsonConvert.DeserializeObject<ApplicationOptions>(optionsText) ?? throw new Exception("Couldn't deserialise ApplicationOptions.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing {0}! Backing up current file and resetting to defaults.", file.FullName);
                    BackupBrokenOptions(file);
                }
            }
            else
            {
                _logger.LogInformation("No file '{0}' exists, using defaults.", file.FullName);
            }
            if (Options == null)
            {
                Options = new ApplicationOptions();
            }
        }

        private void BackupBrokenOptions(FileInfo brokenFile)
        {
            string? backupFileFullName = null;
            try
            {
                if (brokenFile.DirectoryName == null)
                {
                    backupFileFullName = Path.Combine(AssemblyUtil.AssemblyDirectory, brokenFile.Name);
                }
                else
                {
                    backupFileFullName = Path.Combine(brokenFile.DirectoryName, brokenFile.Name);
                }
                int backupNum = 0;
                while (File.Exists(backupFileFullName + backupNum.ToString()))
                {
                    backupNum++;
                }
                File.Copy(brokenFile.FullName, $"{backupFileFullName}{backupNum}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error backing up broken options file '{0}' as '{1}'", brokenFile.FullName, backupFileFullName);
            }
        }
    }
}
