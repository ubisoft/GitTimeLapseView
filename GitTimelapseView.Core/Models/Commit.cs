// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using GitTimelapseView.Core.Common;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Core.Models
{
    public class Commit
    {
        private readonly List<FileChange> _fileChanges = new();

        public Commit(LibGit2Sharp.Commit commit, FileHistory fileHistory, string? remoteUrl)
        {
            Message = commit.Message;
            MessageShort = commit.MessageShort;
            Id = commit.Sha;
            ShortId = Id.Substring(0, 6);
            Author = commit.Author;
            Committer = commit.Committer;
            FileHistory = fileHistory;
            Parents = commit.Parents.Select(x => x.Sha).ToArray();
            if (remoteUrl != null)
            {
                WebUrl = GitHelpers.GetCommitUrl(remoteUrl, commit.Sha);
            }
        }

        public string Message { get; init; }

        public string MessageShort { get; init; }

        public string Id { get; init; }

        public string ShortId { get; init; }

        public string[] Parents { get; init; }

        public Signature Author { get; }

        public Signature Committer { get; }

        public IReadOnlyList<FileChange> FileChanges => _fileChanges;

        public FileHistory FileHistory { get; }

        public string? ContainedInTag { get; private set; }

        public string? WebUrl { get; }

        public void UpdateInfo(ILogger logger)
        {
            if (FileChanges.Any())
                return;

            using (var repository = new Repository(FileHistory.GitRootPath))
            {
                var commit = repository.Lookup<LibGit2Sharp.Commit>(Id);
                if (commit == null)
                    return;

                var parents = commit?.Parents.ToArray() ?? Array.Empty<LibGit2Sharp.Commit>();
                var changes = repository.Diff.Compare<TreeChanges>(parents.FirstOrDefault()?.Tree, commit?.Tree);
                foreach (var change in changes)
                {
                    _fileChanges.Add(new FileChange(this, change));
                }
            }

            if (ContainedInTag != null)
                return;

            var result = GitHelpers.RunGitCommand(FileHistory.GitRootPath, $"describe --contains {Id}", logger);
            if (result != null)
            {
                ContainedInTag = result.Split('~').First();
            }
        }

        public Task UpdateInfoAsync(ILogger logger)
        {
            return Task.Run(() => UpdateInfo(logger));
        }

        public bool IsEqualOrMergeOf(Commit other)
        {
            return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase) || IsMergeOf(other);
        }

        public bool IsMergeOf(Commit other)
        {
            return Parents.Length == 2 && Parents[1].Equals(other.Id, StringComparison.OrdinalIgnoreCase);
        }
    }
}
