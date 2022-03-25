using System.Windows;
using System.Windows.Controls.Primitives;

namespace GitTimelapseView.Wpf.Helpers
{
    public static class FrameworkElementExtensions
    {
        public static void OpenContextMenu(this FrameworkElement frameworkElement)
        {
            var contextMenu = frameworkElement.ContextMenu;
            if (contextMenu == null)
                return;

            contextMenu.PlacementTarget = frameworkElement;
            contextMenu.Placement = PlacementMode.Bottom;

            contextMenu.IsOpen = true;
            if (contextMenu.Items.Count == 0)
            {
                contextMenu.IsOpen = false;
            }
        }
    }
}
