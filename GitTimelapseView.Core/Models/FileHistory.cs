// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using GitTimelapseView.Common;
using GitTimelapseView.Core.Common;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Core.Models
{
    public class FileHistory
    {
        private readonly List<FileRevision> _revisions = new();
        private readonly ConcurrentDictionary<string, FileRevision> _revisionPerCommits = new();

        public FileHistory(string filePath)
        {
            FilePath = filePath;
            try
            {
                GitRootPath = Repository.Discover(filePath).Replace(@".git\", string.Empty, StringComparison.Ordinal);
            }
            catch (Exception)
            {
                // Ignore
            }

            if (string.IsNullOrEmpty(GitRootPath))
            {
                throw new Exception($"{filePath} is not in a valid Git repository");
            }
        }

        public string FilePath { get; }

        public string GitRootPath { get; }

        public IReadOnlyList<FileRevision> Revisions => _revisions;

        public void Initialize(ILogger logger)
        {
            if (Revisions.Any())
                return;

            var commitIds = GetFileCommitIDs(logger).Reverse().ToArray();
            using (var repository = new Repository(GitRootPath))
            {
                var remoteUrl = repository.FindRemoteUrl();

                var relativeFilePath = repository.MakeRelativeFilePath(FilePath);
                if (relativeFilePath == null)
                    throw new Exception($"Unable to blame '{FilePath}'. Path is not located in the repository working directory.");

                for (var index = 0; index < commitIds.Length; index++)
                {
                    var commitId = commitIds[index];
                    var commit = repository.Lookup<LibGit2Sharp.Commit>(commitId.Commit);
                    _revisions.Add(new FileRevision(index, new Commit(commit, this, remoteUrl), commitId.FilePath, this));
                }
            }
        }

        public Task InitializeAsync(ILogger logger)
        {
            return Task.Run(() => Initialize(logger));
        }

        public FileRevision? GetRevisionPerCommit(Commit commit)
        {
            if (_revisionPerCommits.TryGetValue(commit.Id, out var revision))
            {
                return revision;
            }

            revision = Revisions.FirstOrDefault(rev => rev.Commit.IsEqualOrMergeOf(commit));
            if (revision == null)
            {
                using (var repository = new Repository(GitRootPath))
                {
                    revision = Revisions.MinBy(x => repository.Commits.QueryBy(new CommitFilter
                    {
                        IncludeReachableFrom = commit.Id,
                        ExcludeReachableFrom = x.Commit.Id,
                        FirstParentOnly = true,
                    }).Count());
                }
            }

            if (revision != null)
            {
                _revisionPerCommits[commit.Id] = revision;
            }

            return revision;
        }

        private IReadOnlyList<FileCommitId> GetFileCommitIDs(ILogger logger)
        {
            List<FileCommitId> commitIDs = new();
            var isFirstTime = true;

            var filePath = FilePath;
            do
            {
                var args = $"rev-list --first-parent HEAD -- \"{filePath}\"";
                var result = GitHelpers.RunGitCommand(GitRootPath, args, logger).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (result.Count == 0)
                    return commitIDs;

                if (commitIDs.Count > 1 && !isFirstTime)
                {
                    commitIDs.RemoveAt(0);
                }

                commitIDs.AddRange(result.Select(x => new FileCommitId { Commit = x, FilePath = filePath }));

                isFirstTime = false;
            }
            while (GetRenamedPath(logger, commitIDs[commitIDs.Count - 1].Commit, filePath, out filePath));

            return commitIDs;
        }

        private bool GetRenamedPath(ILogger logger, string commitSha1, string currentPath, out string renamedPath)
        {
            renamedPath = string.Empty;

            using (var repository = new Repository(Repository.Discover(FilePath)))
            {
                var parents = repository.Lookup<LibGit2Sharp.Commit>(commitSha1)?.Parents;

                if (parents == null || !parents.Any())
                    return false;
            }

            var commitFilePath = currentPath.Replace(@"\", "/", StringComparison.Ordinal);

            var checkRenameArgs = $"diff --summary {commitSha1}^ {commitSha1}";
            var gitChanges = GitHelpers.RunGitCommand(GitRootPath, checkRenameArgs, logger);

            if (!gitChanges.Contains("rename", StringComparison.OrdinalIgnoreCase))
                return false;

            renamedPath = ParseRenamedPath(gitChanges, commitFilePath);
            return !string.IsNullOrEmpty(renamedPath);
        }

        private string ParseRenamedPath(string rawSummaryLines, string commitFilePath)
        {
            // Renames might give :
            // " rename GitTimelapseView/RenamingTests/App.xaml.cs => GitTimelapseViewNew/RenamingTests/Ap.xaml.cs (100%)"
            // "rename GitTimelapseView/RenamingTests/App.xaml.cs => GitTimelapseViewNew/RenamingTests/Ap.xaml.cs (100%)"
            // " rename {GitTimelapse => GitTimelapseView}/App.xaml.cs (83%)"
            // " rename SwitchToSource.UI/{SwitchWindow.xaml => SwitchPreview/SwitchPreviewWindow.xaml} (94%)"
            var gitFolder = Repository.Discover(FilePath);
            gitFolder = gitFolder.TrimEnd('\\');
            var rootFolder = Path.GetDirectoryName(gitFolder);
            if (rootFolder == null)
                return string.Empty;

            const string BetweenCurlyBracketsPattern = @"\{(.*?)\}";

            var summaryLines = rawSummaryLines.Split('\n');

            foreach (var summaryLine in summaryLines)
            {
                var line = summaryLine;
                var beforeRenamePath = string.Empty;
                var afterRenamePath = string.Empty;

                line = line.TrimStart(' ');

                if (!line.StartsWith("rename", StringComparison.OrdinalIgnoreCase))
                    continue;

                line = line.ReplaceFirst("rename", string.Empty, StringComparison.OrdinalIgnoreCase).TrimStart(' ');
                line = Regex.Replace(line, @"\ \((.*?)\)", string.Empty, RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
                var lineArgs = line.Split(' ');

                var midPathChanges = string.Empty;
                var getBetweenCurlyBracket = Regex.Match(line, BetweenCurlyBracketsPattern, RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
                if (getBetweenCurlyBracket.Success)
                {
                    midPathChanges = getBetweenCurlyBracket.Value;
                }

                if (string.IsNullOrEmpty(midPathChanges))
                {
                    beforeRenamePath = lineArgs[0];
                    afterRenamePath = lineArgs[2];
                    if (!commitFilePath.EndsWith(afterRenamePath, StringComparison.Ordinal))
                        continue;
                    return Path.Combine(rootFolder, beforeRenamePath);
                }

                var remainingPath = Regex.Replace(line, BetweenCurlyBracketsPattern, string.Empty, RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
                midPathChanges = midPathChanges.Replace("{", string.Empty, StringComparison.Ordinal).Replace("}", string.Empty, StringComparison.Ordinal);
                var midPathChangesArgs = midPathChanges.Split(' ');

                if (midPathChangesArgs.Length < 3)
                    throw new FormatException("Unexpected Rename string");

                var afterRenamePart = midPathChangesArgs[2];
                var beforeRenamePart = midPathChangesArgs[0];

                if (remainingPath.StartsWith('/'))
                {
                    afterRenamePath = afterRenamePart + remainingPath;
                    beforeRenamePath = beforeRenamePart + remainingPath;
                }
                else
                {
                    afterRenamePath = remainingPath + afterRenamePart;
                    beforeRenamePath = remainingPath + beforeRenamePart;
                }

                if (commitFilePath.EndsWith(afterRenamePath, StringComparison.Ordinal))
                    return Path.Combine(rootFolder, beforeRenamePath);
            }

            return string.Empty;
        }

        /// <summary>
        /// This is the commit for a given file but since the file path
        /// can change across commits, this keeps the file name at the time the commit was made.
        /// </summary>
        private struct FileCommitId
        {
            public string Commit { get; set; }

            public string FilePath { get; set; }
        }
    }
}
