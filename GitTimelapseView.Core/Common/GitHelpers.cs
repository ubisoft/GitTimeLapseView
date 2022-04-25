using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GitTimelapseView.Common;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Core.Common
{
    public static class GitHelpers
    {
        public static string RunGitCommand(string gitRootPath, string args, ILogger logger, string? onGitErrorMessage = null)
        {
            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,

                FileName = GitExecutableFinder.GitExe,
                Arguments = args,
                CreateNoWindow = true,
                WorkingDirectory = gitRootPath,
            };

            logger.LogInformation($"{processInfo.FileName} {processInfo.Arguments}");

            if (!File.Exists(processInfo.FileName))
            {
                var message = "Could not find git executable. Please add git to your path variables or add path to git executable in plain text in a gitExe.config file in /Users/AppData/Local/GitTimelapseView/ directory";
                throw new FileNotFoundException(message, "git.exe");
            }

            var gitProcess = Process.Start(processInfo);

            if (gitProcess == null)
                throw new InvalidOperationException("Cannot start git.exe");

            gitProcess.EnableRaisingEvents = true;
            gitProcess.Exited += (sender, e) => HandleGitCommandErrors(sender, logger, onGitErrorMessage);

            if (args.Contains("difftool", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return gitProcess.StandardOutput.ReadToEnd().Trim();
        }

        internal static IReadOnlyList<string> GetCommitFileLines(this Repository repository, string relativeFilePath, string sha)
        {
            var commit = repository.Lookup<Commit>(sha);
            if (commit == null)
                return Array.Empty<string>();

            var treeEntry = commit[relativeFilePath];
            var blob = (Blob)treeEntry.Target;
            var lines = new List<string>();

            using (var contentStream = blob.GetContentStream())
            using (var reader = new StreamReader(contentStream, Encoding.UTF8))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        internal static string? MakeRelativeFilePath(this Repository repository, string filePath)
        {
            var fullPath = new Uri(filePath, UriKind.Absolute);
            var relRoot = new Uri(repository.Info.WorkingDirectory, UriKind.Absolute);
            return relRoot.MakeRelativeUri(fullPath).ToString();
        }

        internal static string? FindRemoteUrl(this Repository repository)
        {
            if (repository.Network.Remotes.Any())
            {
                var remote = repository.Network.Remotes.First();
                var url = remote.Url;
                if (url.EndsWith(".git", StringComparison.Ordinal))
                {
                    if (url.StartsWith("git", StringComparison.Ordinal))
                    {
                        url = url.Replace(":", "/", StringComparison.Ordinal).Replace("git@", "https://", StringComparison.Ordinal);
                    }

                    return url.Replace(".git", string.Empty, StringComparison.Ordinal).TrimEnd('/');
                }
            }

            return null;
        }

        internal static string? GetCommitUrl(string remoteUrl, string sha)
        {
            if (remoteUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase))
            {
                return $"{remoteUrl}/commit/{sha}";
            }
            else if (remoteUrl.Contains("gitlab", StringComparison.OrdinalIgnoreCase))
            {
                return $"{remoteUrl}/-/commit/{sha}";
            }

            return null;
        }

        public static string? GetRemotePlatform(string remoteUrl)
        {
            if (remoteUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase))
            {
                return "GitHub";
            }
            else if (remoteUrl.Contains("gitlab", StringComparison.OrdinalIgnoreCase))
            {
                return "GitLab";
            }

            return null;
        }

        private static void HandleGitCommandErrors(object? sender, ILogger logger, string? onGitErrorMessage = null)
        {
            if (sender is Process gitProcess)
            {
                try
                {
                    var standardError = gitProcess.StandardError.ReadToEnd();
                    var stdErrWithoutWarnings = standardError.Split('\n')
                        .Where(line => !line.StartsWith("warning", StringComparison.OrdinalIgnoreCase));

                    var gitErrors = string.Join("\n", stdErrWithoutWarnings);

                    if (!string.IsNullOrEmpty(gitErrors))
                    {
                        var message = $"{onGitErrorMessage ?? string.Empty}" +
                                      $"{Environment.NewLine}{Environment.NewLine}" +
                                      $"Error executing 'git' {gitProcess.StartInfo.Arguments}:" +
                                      $"{Environment.NewLine}{Environment.NewLine}" +
                                      $"{standardError}";
                        logger.LogError(message);
                    }
                }
                finally
                {
                    gitProcess.Dispose();
                }
            }
        }
    }
}
