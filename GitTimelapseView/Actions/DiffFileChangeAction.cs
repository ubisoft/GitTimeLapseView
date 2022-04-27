// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using GitTimelapseView.Core.Common;
using GitTimelapseView.Core.Models;
using GitTimelapseView.Extensions;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Actions
{
    internal class DiffFileChangeAction : ActionBase
    {
        private readonly FileChange _fileChange;
        private readonly string _commitId;

        public DiffFileChangeAction(FileChange fileChange, string commitId)
        {
            DisplayName = "Diff FileChange";
            Tooltip = "Show Diff of a git file change";
            Icon = "VectorDifference";
            _fileChange = fileChange;
            _commitId = commitId;
        }

        public override Task ExecuteAsync(IActionContext context)
        {
            context.TrackingProperties["Path"] = _fileChange.Path;
            context.TrackingProperties["CommitId"] = _commitId;

            context.LogInformation($"Diffing '{_fileChange.Path}' with commit '{_commitId}'");
            var oldPath = string.IsNullOrEmpty(_fileChange.OldPath) ? string.Empty : Path.GetFullPath(Path.Combine(_fileChange.Commit.FileHistory.GitRootPath, _fileChange.OldPath));
            var path = string.IsNullOrEmpty(_fileChange.Path) ? string.Empty : Path.GetFullPath(Path.Combine(_fileChange.Commit.FileHistory.GitRootPath, _fileChange.Path));

            var errorMessage = $"Could not launch difftool on {_fileChange.Path} for commit {_commitId}";
            var args = path.Equals(oldPath, System.StringComparison.OrdinalIgnoreCase) ? $"difftool -y {_commitId}^ {_commitId} {path}".Trim(' ') : $"difftool -y {_commitId}^ {_commitId} {oldPath} {path}".Trim(' ');
            GitHelpers.RunGitCommand(_fileChange.Commit.FileHistory.GitRootPath, args, context, errorMessage);

            return Task.CompletedTask;
        }
    }
}
