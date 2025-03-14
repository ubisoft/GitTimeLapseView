using System.Globalization;
using AntDesign;
using GitTimelapseView.Actions;
using GitTimelapseView.Core.Models;
using GitTimelapseView.Helpers;
using GitTimelapseView.Services;
using Microsoft.AspNetCore.Components;

namespace GitTimelapseView;

public partial class CommitInfo
{
    [Inject] private TimelapseService TimelapseService { get; set; } = default!;

    [Inject] private MessageService MessageService { get; set; } = default!;
    private Commit? _commit;
    private string _authoredString => _commit?.Author.When.ToString("dd/MM/yyyy @ hh:mm", CultureInfo.InvariantCulture) ?? string.Empty;
    private const string CopyToClipboardTooltip = "Copy complete id to clipboard";

    protected override async Task OnInitializedAsync()
    {
        TimelapseService.CurrentCommitChanged += async (_, _) => await UpdateAsync().ConfigureAwait(false);
        await UpdateAsync().ConfigureAwait(false);
        await base.OnInitializedAsync().ConfigureAwait(false);
    }

    private async Task UpdateAsync()
    {
        if (TimelapseService.FileHistory != null && TimelapseService.CurrentCommit != null)
        {
            _commit = TimelapseService.CurrentCommit;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnButtonClicked()
    {
        if (TimelapseService.CurrentCommit != null)
        {
            await new CopyToClipboardAction(TimelapseService.CurrentCommit.Id, "commit id").ExecuteAsync().ConfigureAwait(false);
        }
    }

    private void OnWebUrlClicked()
    {
        if (_commit?.WebUrl != null)
        {
            Process.Start(new ProcessStartInfo { FileName = _commit?.WebUrl, UseShellExecute = true, });
        }
    }
}
