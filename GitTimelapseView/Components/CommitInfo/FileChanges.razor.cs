using GitTimelapseView.Actions;
using GitTimelapseView.Core.Models;
using GitTimelapseView.Helpers;
using LibGit2Sharp;
using Microsoft.AspNetCore.Components;

namespace GitTimelapseView;

public partial class FileChanges
{
    private const string DiffTooltip = "View diff using default diff tool";
    private FileChangeTableRow[] Rows => Changes.Select(x => new FileChangeTableRow(x)).ToArray();
    private const int PageSize = 100;
    private bool _hidePagination => Changes.Count < PageSize;
    private string _scrollY => _hidePagination ? "220px" : "160px";

    [Parameter] public string CommitId { get; set; } = default!;

    [Parameter] public IReadOnlyList<FileChange> Changes { get; set; } = default!;

    public void OnRowClicked(FileChangeTableRow fileChangeRow)
    {
        _ = new DiffFileChangeAction(fileChangeRow.FileChange, CommitId).ExecuteAsync().ConfigureAwait(false);
    }

    public void OnPathClicked(FileChangeTableRow fileChangeRow)
    {
        _ = new OpenFileAction(fileChangeRow.Path, revisionSha: fileChangeRow.FileChange.Commit.Id).ExecuteAsync().ConfigureAwait(false);
    }

    public class FileChangeTableRow
    {
        public FileChangeTableRow(FileChange fileChange)
        {
            ChangeKind = fileChange.ChangeKind;
            Path = fileChange.Path;
            FileChange = fileChange;
        }

        public ChangeKind ChangeKind { get; set; }

        public string Path { get; set; }

        public FileChange FileChange { get; }

        public string RowClass => "filechange";
    }
}
