using System.Collections.Generic;

namespace GitTimelapseView.Extensions
{
    public interface ITitleBarActionProvider
    {
        /// <summary>
        /// Provide actions that will be added to title bar
        /// </summary>
        IEnumerable<IAction> GetActions();
    }
}
