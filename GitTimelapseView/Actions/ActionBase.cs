using System;
using System.Threading.Tasks;
using GitTimelapseView.Extensions;

namespace GitTimelapseView.Actions
{
    public abstract class ActionBase : IAction
    {
        public string DisplayName { get; set; } = string.Empty;

        public string? Tooltip { get; set; }

        public object? Icon { get; set; }

        public virtual IAction[] Children { get; set; } = Array.Empty<IAction>();

        public string? InputGestureText { get; set; } = string.Empty;

        public string? SuccessNotificationText { get; set; }

        public string? FailureNotificationText { get; set; }

        public abstract Task ExecuteAsync(IActionContext context);
    }
}
