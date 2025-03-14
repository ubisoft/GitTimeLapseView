using AntDesign;
using GitTimelapseView.Extensions;
using Microsoft.AspNetCore.Components;

namespace GitTimelapseView;

public partial class UserTooltip
{
    [Parameter] public UserInfo UserInfo { get; set; } = default!;

    [Parameter] public RenderFragment ChildContent { get; set; } = default!;

    [Parameter] public Placement Placement { get; set; } = AntDesign.Placement.Top;

    private string Title
    {
        get
        {
            if (!string.IsNullOrEmpty(UserInfo.DisplayName))
            {
                return UserInfo.DisplayName;
            }

            if (!string.IsNullOrEmpty(UserInfo.AccountName))
            {
                return UserInfo.AccountName;
            }

            return UserInfo.Email;
        }
    }
}
