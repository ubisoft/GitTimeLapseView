// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GitTimelapseView.Actions;
using GitTimelapseView.Extensions;
using GitTimelapseView.Helpers;
using GitTimelapseView.Services;
using GitTimelapseView.Wpf.Helpers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace GitTimelapseView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly TimelapseService? _timelapseService;
        private readonly ThemingService? _themingService;

        public MainWindow(TimelapseService? timelapseService = null, ThemingService? themingService = null)
        {
            var rootDir = Path.GetDirectoryName(typeof(MainWindow).Assembly.Location);
            HostPage = rootDir != null ? Path.Combine(rootDir, @"wwwroot\index.html") : @"wwwroot\index.html";
            Loaded += MainWindow_Loaded;
            InitializeComponent();
            _timelapseService = timelapseService;
            _themingService = themingService;
            if (timelapseService != null)
            {
                timelapseService.FileLoading += (sender, args) => UpdateTitle();
                timelapseService.CurrentFileRevisionIndexChanged += (sender, args) => UpdateTitle();
            }

            UpdateTitle();
            SourceInitialized += MainWindow_SourceInitialized;
            Closing += MainWindow_Closing;
            Reload();
        }

        public string HostPage { get; }

        public void ExitApplication()
        {
            Close();
            Application.Current.Shutdown();
        }

        internal void Reload()
        {
            var webView = new BlazorWebView
            {
                HostPage = HostPage,
                Services = App.Current.ServiceProvider,
                Background = Brushes.Red,
            };
            webView.RootComponents.Add(new RootComponent() { Selector = "head::after", ComponentType = typeof(HeadOutlet) });

            var rootComponent = new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(RazorApp),
            };
            webView.RootComponents.Add(rootComponent);
            _webViewGrid.Children.Clear();
            _webViewGrid.Children.Add(webView);
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = Top;
                Properties.Settings.Default.Left = Left;
                Properties.Settings.Default.Height = Height;
                Properties.Settings.Default.Width = Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }

        private void MainWindow_SourceInitialized(object? sender, System.EventArgs e)
        {
            if (Properties.Settings.Default.Height > 0 && Properties.Settings.Default.Width > 0)
            {
                Top = Properties.Settings.Default.Top;
                Left = Properties.Settings.Default.Left;
                Height = Properties.Settings.Default.Height;
                Width = Properties.Settings.Default.Width;
                if (Properties.Settings.Default.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateWindowButtonStates();
            StateChanged += (x, args) => UpdateWindowButtonStates();
            InitializeActions();
        }

        private void InitializeActions()
        {
            var providers = App.Current.ServiceProvider.GetServices<ITitleBarActionProvider>();
            foreach (var provider in providers)
            {
                var commands = provider.GetActions();
                foreach (var command in commands)
                {
                    var button = command.ToButton();
                    button.Style = (Style)FindResource("TitleBarButtonStyle");
                    AdditionalCommandsItemsControl.Items.Add(button);
                }
            }

            FileMenu.Items.Add(new OpenFileAction().ToMenuItem());
            FileMenu.Items.Add(new Separator());
            FileMenu.Items.Add(new ExitApplicationAction().ToMenuItem());
            HelpMenu.Items.Add(new ViewLogsAction().ToMenuItem());
            HelpMenu.Items.Add(new AboutAction().ToMenuItem());
            ViewMenu.Items.Add(CreateAppearanceMenu());
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            new OpenFileAction().ExecuteAsync().ConfigureAwait(false);
        }

        private void CloseButton_Executed(object sender, ExecutedRoutedEventArgs e) => ExitApplication();

        private void CloseButton_OnClick(object sender, RoutedEventArgs e) => ExitApplication();

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void RestoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void UpdateWindowButtonStates()
        {
            MaximizeButton.Visibility = WindowState == WindowState.Maximized ? Visibility.Collapsed : Visibility.Visible;
            RestoreButton.Visibility = WindowState == WindowState.Normal ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateTitle()
        {
            string title;
            if (_timelapseService != null && !string.IsNullOrEmpty(_timelapseService.FilePath))
            {
                title = $"{Path.GetFileName(_timelapseService.FilePath)}#{_timelapseService.CurrentFileRevision?.Label} - {App.Current.ApplicationName}";
            }
            else
            {
                title = nameof(GitTimelapseView);
            }

            Dispatcher.Invoke(() => Title = title);
        }

        private MenuItem CreateAppearanceMenu()
        {
            var appearanceMenuItem = new MenuItem
            {
                Header = "Appearance",
            };
            if (_themingService != null)
            {
                foreach (var theme in _themingService.Themes)
                {
                    var themeMenuItem = new MenuItem
                    {
                        Header = theme.Name,
                        IsCheckable = true,
                        IsChecked = _themingService.Theme == theme,
                        Tag = theme,
                    };
                    themeMenuItem.Click += (sender, args) =>
                    {
                        _themingService.ApplyTheme(theme);
                        UpdateCheckedState(appearanceMenuItem);
                    };
                    appearanceMenuItem.Items.Add(themeMenuItem);
                }
            }

            return appearanceMenuItem;

            void UpdateCheckedState(MenuItem menuItem)
            {
                foreach (var item in menuItem.Items.OfType<MenuItem>())
                {
                    item.IsChecked = item.Tag is ThemeInfo themeInfo && _themingService.Theme == themeInfo;
                }
            }
        }
    }

#pragma warning disable MA0048,SA1601,SA1402
    public partial class RazorApp
    {
    }
#pragma warning restore SA1402,SA1601,MA0048
}
