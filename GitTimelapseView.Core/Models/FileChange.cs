// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using LibGit2Sharp;

namespace GitTimelapseView.Core.Models
{
    public sealed class FileChange
    {
        public FileChange(Commit commit, TreeEntryChanges change, string workingDirectory)
        {
            Commit = commit;
            ChangeKind = change.Status;
            Path = string.IsNullOrEmpty(change.Path) ? string.Empty : System.IO.Path.GetFullPath(System.IO.Path.Combine(workingDirectory, change.Path));
            OldPath = string.IsNullOrEmpty(change.OldPath) ? string.Empty : System.IO.Path.GetFullPath(System.IO.Path.Combine(workingDirectory, change.OldPath));
            Name = System.IO.Path.GetFileName(Path);
        }

        public ChangeKind ChangeKind { get; }

        public Commit Commit { get; }

        public string Name { get; }

        public string Path { get; }

        public string OldPath { get; }
    }
}
