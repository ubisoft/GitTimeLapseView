// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using LibGit2Sharp;

namespace GitTimelapseView.Core.Models
{
    public sealed class FileChange
    {
        public FileChange(Commit commit, TreeEntryChanges change)
        {
            Commit = commit;
            ChangeKind = change.Status;
            Path = change.Path;
            OldPath = change.OldPath;
            Name = System.IO.Path.GetFileName(Path);
        }

        public ChangeKind ChangeKind { get; }

        public Commit Commit { get; }

        public string Name { get; }

        public string Path { get; }

        public string OldPath { get; }
    }
}
