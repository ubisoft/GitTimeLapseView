using AntDesign;
using GitTimelapseView.Core.Models;
using GitTimelapseView.Services;
using Microsoft.AspNetCore.Components;

namespace GitTimelapseView;

public partial class RevisionTooltip
{
    [Inject] public TimelapseService? TimelapseService { get; set; } = default!;

    [Parameter] public FileRevision Revision { get; set; } = default!;

    [Parameter] public RenderFragment ChildContent { get; set; } = default!;

    [Parameter] public Placement Placement { get; set; } = Placement.Top;
    private string Title => $"Revision {Revision.Label}";
}
