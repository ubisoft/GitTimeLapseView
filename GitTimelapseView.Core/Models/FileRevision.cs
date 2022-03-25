using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitTimelapseView.Core.Common;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Core.Models
{
    public class FileRevision
    {
        public FileRevision(int index, Commit commit, FileHistory fileHistory)
        {
            Index = index;
            FileHistory = fileHistory;
            Commit = commit;
        }

        public Commit Commit { get; }

        public int Index { get; }

        public string Label => $"{Index + 1}";

        public FileHistory FileHistory { get; }

        public IList<BlameBlock> Blocks { get; } = new List<BlameBlock>();

        public void LoadBlocks(ILogger logger)
        {
            if (Blocks.Any())
                return;

            using (var repository = new Repository(FileHistory.GitRootPath))
            {
                var relativeFilePath = repository.MakeRelativeFilePath(FileHistory.FilePath);
                if (relativeFilePath == null)
                    throw new Exception($"Unable to blame '{FileHistory.FilePath}'. Path is not located in the repository working directory.");

                var blocks = repository.Blame(relativeFilePath, new BlameOptions { StartingAt = Commit.Id });
                var lines = repository.GetCommitFileLines(relativeFilePath, Commit.Id);

                foreach (var block in blocks)
                {
                    Blocks.Add(new BlameBlock(block, this, lines));
                }
            }
        }

        public async Task LoadBlocksAsync(ILogger logger)
        {
            await Task.Run(() => LoadBlocks(logger)).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return $"#{Index} {Commit.ShortId} {Commit.MessageShort}";
        }
    }
}
