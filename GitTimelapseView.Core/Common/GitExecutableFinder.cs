using System;
using System.IO;
using Microsoft.Win32;

namespace GitTimelapseView.Common
{
    public static class GitExecutableFinder
    {
        private static readonly string _configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitTimelapseView", "gitExe.config");

        private static string? _gitExe;

        private static string? _gitBashRootDirectory;

        public static string? GitExe
        {
            get
            {
                if (string.IsNullOrEmpty(_gitExe))
                {
                    _gitExe = FindGitSystemToolset();
                }

                return _gitExe;
            }
        }

        public static string? GitBashRootDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_gitBashRootDirectory))
                {
                    _gitBashRootDirectory = GitBashRootDirectoryImpl();
                }

                return _gitBashRootDirectory;
            }
        }

        private static string? FindGitExeInConfigFile()
        {
            if (!File.Exists(_configFile))
                return null;

            var configFileContent = File.ReadAllText(_configFile);

            configFileContent = AddQuotesIfContainsSpaces(configFileContent);

            return configFileContent.Contains("git.exe", StringComparison.OrdinalIgnoreCase) && File.Exists(configFileContent)
                ? configFileContent
                : null;
        }

        private static string AddQuotesIfContainsSpaces(string str)
        {
            if (!str.Contains(' ', StringComparison.Ordinal))
                return str;

            if (!str.StartsWith('"'))
            {
                str = str.Insert(0, "\"");
            }

            if (!str.EndsWith('"'))
            {
                str += "\"";
            }

            return str;
        }

        private static string? FindGitExeInPathVariable()
        {
            var usualPaths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)?.Split(';') ?? Array.Empty<string>();

            foreach (var path in usualPaths)
            {
                var gitExecutablePath = Path.Combine(path, "git.exe");

                if (File.Exists(gitExecutablePath))
                    return gitExecutablePath;
            }

            return null;
        }

        private static string? FindGitSystemToolset()
        {
            var gitPath = FindGitExeInConfigFile();

            if (!string.IsNullOrEmpty(gitPath) && File.Exists(gitPath))
                return gitPath;

            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                gitPath = FindGitExeInPathVariable();
            }
            else
            {
                var defaultPath = Environment.ExpandEnvironmentVariables(@"%ProgramW6432%\git\cmd\git.exe");
                gitPath = FindGitExeInPathVariable();

                if (File.Exists(defaultPath))
                    return defaultPath;
            }

            if (gitPath != null && File.Exists(gitPath))
                return gitPath;

            var gitBashRoot = GitBashRootDirectory;
            return gitBashRoot != null ? Path.Combine(gitBashRoot, "cmd", "git.exe") : null;
        }

        private static string? GitBashRootDirectoryImpl()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                foreach (var rootKey in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
                {
                    foreach (var registryView in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                    {
                        var subKeyName = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
                        var uninstallPath = FindGitInstallPathInRegistry(RegistryKey.OpenBaseKey(rootKey, registryView), subKeyName);
                        if (uninstallPath != null)
                            return uninstallPath;
                    }
                }
            }

            return null;
        }

        private static string? FindGitInstallPathInRegistry(RegistryKey root, string keyName)
        {
            using (var uninstall = root.OpenSubKey(keyName, writable: false))
            {
                if (uninstall == null)
                    return null;

                foreach (var subKeyName in uninstall.GetSubKeyNames())
                {
                    using (var softwareKey = uninstall.OpenSubKey(subKeyName))
                    {
                        if (softwareKey == null)
                            continue;

                        if (softwareKey.GetValue("DisplayName") is not string displayName || !displayName.StartsWith("Git version ", StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (softwareKey.GetValue("InstallLocation") is string installLocation && File.Exists($@"{installLocation}\cmd\git.exe"))
                            return installLocation;
                    }
                }
            }

            return null;
        }
    }
}
