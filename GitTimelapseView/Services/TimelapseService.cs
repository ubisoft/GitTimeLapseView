// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using GitTimelapseView.Core.Models;
using GitTimelapseView.Data;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Services
{
    public class TimelapseService : ServiceBase
    {
        private FileRevision? _currentFileRevision;
        private Commit? _currentCommit;
        private string? _filePath;

        public TimelapseService(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public event EventHandler<string>? FileLoading;

        public event EventHandler<string>? FileLoaded;

        public event EventHandler<FileRevisionIndexChangedEventArgs>? CurrentFileRevisionIndexChanged;

        public event EventHandler<FileRevisionIndexChangedEventArgs>? CurrentFileRevisionIndexChanging;

        public event EventHandler<CommitChangedEventArgs>? CurrentCommitChanging;

        public event EventHandler<CommitChangedEventArgs>? CurrentCommitChanged;

        public string? FilePath
        {
            get => _filePath;
            private set => SetProperty(ref _filePath, value);
        }

        public FileHistory? FileHistory { get; private set; }

        public FileRevision? CurrentFileRevision
        {
            get => _currentFileRevision;
            private set => SetProperty(ref _currentFileRevision, value);
        }

        public int CurrentFileRevisionIndex => CurrentFileRevision?.Index ?? 0;

        public Commit? CurrentCommit
        {
            get => _currentCommit;
            private set => SetProperty(ref _currentCommit, value);
        }

        public int? InitialLineNumber { get; set; }

        public async Task OpenFileAsync(ILogger logger, string filePath, int? lineNumber, int? revisionIndex, string? revisionSha)
        {
            var oldCommitId = CurrentCommit;
            var oldFilePath = FilePath;
            try
            {
                FileLoading?.Invoke(this, filePath);
                CurrentCommit = null;
                CurrentFileRevision = null;
                FilePath = filePath;

                FileHistory = new FileHistory(filePath);
                await FileHistory.InitializeAsync(logger).ConfigureAwait(false);

                InitialLineNumber = lineNumber ?? 0;
                if (FileHistory.Revisions.Count > 0)
                {
                    var revisionIndexToSelect = FileHistory.Revisions.Count - 1;
                    if (revisionIndex != null && revisionIndex > 0 && revisionIndex < FileHistory.Revisions.Count)
                    {
                        revisionIndexToSelect = revisionIndex.Value;
                    }
                    else if (revisionSha != null)
                    {
                        var revision = FileHistory.Revisions.FirstOrDefault(x => string.Equals(x.Commit.Id, revisionSha, StringComparison.Ordinal));
                        if (revision != null)
                        {
                            revisionIndexToSelect = revision.Index;
                        }
                    }

                    await SetCurrentFileRevisionIndexAsync(logger, revisionIndexToSelect, FileRevisionIndexChangeReason.Loading).ConfigureAwait(false);
                }

                FileLoaded?.Invoke(this, filePath);
            }
            catch (Exception e)
            {
                CurrentCommit = oldCommitId;
                FilePath = oldFilePath;
                logger.LogError(e, $"Unable to open file '{filePath}'");
                throw;
            }
        }

        public async Task SetCurrentFileRevisionIndexAsync(ILogger logger, int index, FileRevisionIndexChangeReason reason = FileRevisionIndexChangeReason.Explicit)
        {
            if (FileHistory == null)
            {
                logger.LogError($"Unable to set current file revision index to {index}. No {nameof(FileHistory)}");
                return;
            }

            try
            {
                CurrentFileRevisionIndexChanging?.Invoke(this, new FileRevisionIndexChangedEventArgs(index, reason));
                CurrentFileRevision = FileHistory.Revisions[index];
                await CurrentFileRevision.LoadBlocksAsync(logger).ConfigureAwait(false);
                CurrentCommit = CurrentFileRevision.Commit;
                await CurrentCommit.UpdateInfoAsync(logger).ConfigureAwait(false);
                CurrentFileRevisionIndexChanged?.Invoke(this, new FileRevisionIndexChangedEventArgs(index, reason));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unable to set current file revision index to {index}");
            }
        }

        public async Task SetCurrentCommitAsync(ILogger logger, Commit commit, CommitChangeReason reason = CommitChangeReason.Explicit)
        {
            try
            {
                CurrentCommitChanging?.Invoke(this, new CommitChangedEventArgs(commit, reason));
                CurrentCommit = commit;
                await commit.UpdateInfoAsync(logger).ConfigureAwait(false);
                CurrentCommitChanged?.Invoke(this, new CommitChangedEventArgs(commit, reason));
            }
            catch (Exception e)
{
                logger.LogError(e, $"Unable to set current commit to {commit.Id}");
            }
        }
    }
}
