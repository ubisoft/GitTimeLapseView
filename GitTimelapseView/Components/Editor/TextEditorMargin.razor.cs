using GitTimelapseView.Actions;
using GitTimelapseView.Core.Models;
using GitTimelapseView.Helpers;
using GitTimelapseView.Services;
using Microsoft.AspNetCore.Components;

namespace GitTimelapseView;

public partial class TextEditorMargin
{
    private const int RowHeight = 19;

    [Parameter] public string CssClass { get; set; } = "";

    [Inject] private TimelapseService TimelapseService { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        TimelapseService.CurrentFileRevisionIndexChanged += async (_, _) => await InvokeAsync(StateHasChanged);
        return base.OnInitializedAsync();
    }

    private async Task OnBlockClicked(BlameBlock block)
    {
        await new SelectCommitAction(block.InitialCommit).ExecuteAsync();
    }

    private string getBlockStyle(BlameBlock block) => $"height:{RowHeight * block.LineCount}px;max-height:{RowHeight * block.LineCount}px;margin:0px;";
}
