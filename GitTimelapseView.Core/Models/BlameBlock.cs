﻿// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using GitTimelapseView.Common;
using LibGit2Sharp;

namespace GitTimelapseView.Core.Models
{
    public class BlameBlock
    {
        public BlameBlock(BlameHunk block, FileRevision fileRevision, IReadOnlyList<string> lines, string? remoteUrl)
        {
            InitialSignature = block.InitialSignature;
            FinalSignature = block.FinalSignature;
            InitialCommit = new Commit(block.InitialCommit, fileRevision.FileHistory, remoteUrl);
            FinalCommit = new Commit(block.FinalCommit, fileRevision.FileHistory, remoteUrl);
            StartLine = block.FinalStartLineNumber + 1;
            LineCount = block.LineCount;
            FileRevision = fileRevision.FileHistory.GetRevisionPerCommit(FinalCommit);
            Lines = lines;
            Text = lines.ConcatLines(block.FinalStartLineNumber, block.LineCount).Trim('\n');
        }

        public Signature InitialSignature { get; }

        public Signature FinalSignature { get; }

        public Commit InitialCommit { get; }

        public Commit FinalCommit { get; }

        public int StartLine { get; }

        public int LineCount { get; }

        public FileRevision? FileRevision { get; }

        public IReadOnlyList<string> Lines { get; }

        public string Text { get; }
    }
}
