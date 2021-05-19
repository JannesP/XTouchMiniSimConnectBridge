using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace JannesP.DeviceSimConnectBridge.MsfsModule.BuildHelper
{
    public static class Program
    {
        public static Dictionary<string, string>? ParsedArgs { get; private set; }

        public static void DeployModule()
        {
            if (ParsedArgs!.TryGetValue("-Output", out string? outputDir) && ParsedArgs!.TryGetValue("-Deploy", out string? deployDir))
            {
                Log($"Deploying from {outputDir} to {deployDir} ...");
                if (!Directory.Exists(deployDir))
                {
                    Directory.CreateDirectory(deployDir);
                }
                CopyAll(outputDir, deployDir);
            }
            else
            {
                Log("Skipping Deployment -Output or -Deploy missing ...");
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                Log($"MsfsModuleBuildHelper started with: {string.Join(", ", args)}");
                ParseArgs(args);
                PackModule();
                DeployModule();
            }
            catch (ProgramExitException ex)
            {
                Console.WriteLine("Error occured: " + ex.ToString());
                Environment.Exit(ex.ExitCode);
            }
            catch (Exception ex)
            {
                Log($"Unspecified error occured: " + ex.ToString());
                Environment.Exit(-1);
            }
        }

        private static void CopyAll(string sourcePath, string targetPath)
        {
            var source = new DirectoryInfo(sourcePath);
            var target = new DirectoryInfo(targetPath);
            CopyAll(source, target);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private static void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        private static void PackModule()
        {
            if (ParsedArgs!.TryGetValue("-Manifest", out string? manifestFilePath) && ParsedArgs!.TryGetValue("-Modules", out string? modulesPath) && ParsedArgs!.TryGetValue("-Output", out string? outputPath))
            {
                Log("Packing module ...");
                if (!File.Exists(manifestFilePath))
                {
                    throw new ProgramExitException(-1, $"Manifest file {manifestFilePath} not found.");
                }
                if (!Directory.Exists(modulesPath))
                {
                    throw new ProgramExitException(-1, $"Modules path {modulesPath} not found.");
                }

                string[] moduleFiles = Directory.GetFiles(modulesPath, "*.wasm");
                object layoutStructure = new {
                    content = moduleFiles.Select(f => new {
                        path = "modules/" + Path.GetFileName(f),
                        size = new FileInfo(f).Length,
                        date = File.GetLastWriteTime(f).ToFileTimeUtc(),
                    }).ToArray(),
                };

                if (!Directory.Exists(outputPath))
                {
                    Log($"Creating output directory {outputPath} ...");
                    Directory.CreateDirectory(outputPath);
                }
                string modulesDir = Path.Combine(outputPath, "modules");
                if (!Directory.Exists(modulesDir))
                {
                    Log($"Creating modules directory {modulesDir} ...");
                    Directory.CreateDirectory(modulesDir);
                }
                foreach (string file in moduleFiles)
                {
                    File.Copy(file, Path.Combine(modulesDir, Path.GetFileName(file)), true);
                }
                File.Copy(manifestFilePath, Path.Combine(outputPath, "manifest.json"), true);
                File.WriteAllText(Path.Combine(outputPath, "layout.json"), JsonConvert.SerializeObject(layoutStructure, Formatting.Indented));
            }
            else
            {
                Log("No pack possible, -Manifest -Modules and -Output are required. Skipping ...");
            }
        }

        [MemberNotNull(nameof(ParsedArgs))]
        private static void ParseArgs(string[] args)
        {
            var dict = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                switch (arg)
                {
                    case "-Manifest":
                    case "-Modules":
                    case "-Output":
                    case "-Deploy":
                        if (args.Length + 1 < i || args[i + 1].StartsWith("-"))
                        {
                            throw new ProgramExitException(-1, $"Option {args[i]} didn't have the required argument.");
                        }
                        dict.Add(arg, args[++i]);
                        break;
                }
            }

            ParsedArgs = dict;
        }

        private class ProgramExitException : Exception
        {
            public ProgramExitException(int exitCode, string message) : base(message)
            {
                ExitCode = exitCode;
            }

            public int ExitCode { get; set; }
        }
    }
}