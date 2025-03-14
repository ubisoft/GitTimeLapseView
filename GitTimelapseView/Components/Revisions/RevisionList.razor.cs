using AntDesign.TableModels;
using GitTimelapseView.Actions;
using GitTimelapseView.Core.Models;
using GitTimelapseView.Helpers;
using GitTimelapseView.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GitTimelapseView;

public partial class RevisionList
{
    private const string RevisionTooltip = "Click to change current revision";
    private FileRevisionTableRow[] Revisions => TimelapseService.FileHistory != null ? TimelapseService.FileHistory.Revisions.Reverse().Select(x => new FileRevisionTableRow(x, TimelapseService)).ToArray() : [];

    [Inject] private TimelapseService TimelapseService { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        TimelapseService.FileLoaded += async (_, _) => await RefreshAsync();
        TimelapseService.CurrentFileRevisionIndexChanging += async (_, _) => await RefreshAsync();
        return base.OnInitializedAsync();
    }

    private Dictionary<string, object> OnRow(RowData<FileRevisionTableRow> row)
    {
        void OnClick(MouseEventArgs args)
        {
            _ = new SelectCommitAction(row.Data.Revision.Commit).ExecuteAsync().ConfigureAwait(false);
        }

        void OnDoubleClick(MouseEventArgs args)
        {
            _ = new ChangeCurrentRevisionAction(row.Data.Index).ExecuteAsync().ConfigureAwait(false);
        }

        return new Dictionary<string, object> { { "ondoubleclick", (Action<MouseEventArgs>)OnDoubleClick }, { "onclick", (Action<MouseEventArgs>)OnClick }, };
    }

    public Task RefreshAsync()
    {
        return InvokeAsync(StateHasChanged);
    }

    public void OnNavigate(FileRevisionTableRow fileChangeRow)
    {
        _ = new ChangeCurrentRevisionAction(fileChangeRow.Index).ExecuteAsync().ConfigureAwait(false);
    }

    public class FileRevisionTableRow
    {
        private readonly TimelapseService _timelapseService;

        public FileRevisionTableRow(FileRevision revision, TimelapseService timelapseService)
        {
            Index = revision.Index;
            Id = revision.Commit.Id;
            ShortId = revision.Commit.ShortId;
            Message = revision.Commit.Message;
            AuthorDate = revision.Commit.Committer.When.LocalDateTime;
            AuthorEmail = revision.Commit.Author.Email;
            Revision = revision;
            _timelapseService = timelapseService;
        }

        public int Index { get; set; }

        public string Id { get; set; }

        public string ShortId { get; set; }

        public string Message { get; set; }

        public DateTime AuthorDate { get; set; }

        public string AuthorEmail { get; set; }

        public string RowClass => _timelapseService.CurrentFileRevisionIndex == Index ? "current-revision revision" :
            _timelapseService.CurrentFileRevisionIndex < Index ? "future-revision revision" : "past-revision revision";

        public FileRevision Revision { get; }
    }
}
