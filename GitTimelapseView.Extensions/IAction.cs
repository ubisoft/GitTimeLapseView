using System.Threading.Tasks;

namespace GitTimelapseView.Extensions
{
    public interface IAction
    {
        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the action tooltip
        /// </summary>
        string? Tooltip { get; set; }

        /// <summary>
        /// Gets or sets the icon of the action. It can be a string representing an icon from MaterialDesignIcons, or an image url
        /// </summary>
        object? Icon { get; set; }

        /// <summary>
        /// Gets or sets the text describing an input gesture that will call the command tied to the specified item.
        /// </summary>
        string? InputGestureText { get; set; }

        /// <summary>
        /// Gets or sets the success notification text
        /// </summary>
        string? SuccessNotificationText { get; set; }

        /// <summary>
        /// Gets or sets the success notification text
        /// </summary>
        string? FailureNotificationText { get; set; }

        /// <summary>
        /// Gets or sets the children actions
        /// </summary>
        IAction[] Children { get; set; }

        /// <summary>
        /// Code that will be executed when action is started
        /// </summary>
        /// <param name="context">the action context.</param>
        /// <returns>An execution task.</returns>
        Task ExecuteAsync(IActionContext context);
    }
}
