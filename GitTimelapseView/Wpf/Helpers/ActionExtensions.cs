// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GitTimelapseView.Extensions;
using GitTimelapseView.Helpers;
using MaterialDesignThemes.Wpf;

namespace GitTimelapseView.Wpf.Helpers
{
    internal static class ActionExtensions
    {
        public static Button ToButton(this IAction action)
        {
            var button = new Button
            {
                ToolTip = action.DisplayName,
            };
            if (action.Children.Any())
            {
                var content = new StackPanel { Orientation = Orientation.Horizontal };
                content.Children.Add(GetIcon(action.Icon));
                content.Children.Add(new PackIcon { Kind = PackIconKind.MenuDown });
                button.Content = content;

                var contextMenu = new ContextMenu();
                foreach (var child in action.Children)
                {
                    contextMenu.Items.Add(child.ToMenuItem());
                }

                button.ContextMenu = contextMenu;
                button.Click += (sender, args) => button.OpenContextMenu();
            }
            else
            {
                button.Content = GetIcon(action.Icon);
                button.Click += async (sender, args) => await action.ExecuteAsync().ConfigureAwait(false);
            }

            return button;
        }

        public static MenuItem ToMenuItem(this IAction action)
        {
            var menuItem = new MenuItem
            {
                Header = action.DisplayName,
                ToolTip = action.Tooltip,
                Icon = GetIcon(action.Icon),
                InputGestureText = action.InputGestureText,
            };
            menuItem.Click += async (sender, args) => await action.ExecuteAsync().ConfigureAwait(false);
            foreach (var child in action.Children)
            {
                menuItem.Items.Add(child.ToMenuItem());
            }

            return menuItem;
        }

        public static UIElement? GetIcon(object? icon)
        {
            if (icon is string str)
            {
                if (Enum.TryParse<PackIconKind>(str, out var packIconKind))
                {
                    return new PackIcon { Kind = packIconKind };
                }
                else
                {
                    return new Image { Source = new BitmapImage(new Uri(str)), Width = 20, Height = 20 };
                }
            }

            return null;
        }
    }
}
