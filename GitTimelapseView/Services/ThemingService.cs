// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;

namespace GitTimelapseView.Services
{
    public class ThemingService : ServiceBase
    {
        private readonly ThemeInfo _lightTheme = new("Light")
        {
            MonacoTheme = "vs",
        };

        private readonly ThemeInfo _darkTheme = new("Dark")
        {
            IsDark = true,
            MonacoTheme = "vs-dark",
        };

        public ThemingService(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            Themes =
            [
                _lightTheme,
                _darkTheme
            ];
            var themeProperty = Properties.Settings.Default.Theme;
            var theme = !string.IsNullOrEmpty(themeProperty)
                ? Themes.FirstOrDefault(x => x.Name.Equals(themeProperty, StringComparison.OrdinalIgnoreCase)) ?? Themes[0]
                : Themes[0];

            Theme = theme;
            ApplyTheme(theme, saveSettings: false, reloadWindow: false);
        }

        public ThemeInfo Theme { get; private set; }

        public ThemeInfo[] Themes { get; }

        public void ApplyTheme(ThemeInfo themeInfo, bool saveSettings = true, bool reloadWindow = true)
        {
            Theme = themeInfo;
            if (saveSettings)
            {
                Properties.Settings.Default.Theme = themeInfo.Name;
                Properties.Settings.Default.Save();
            }

            if (reloadWindow && App.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Reload();
            }

            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            if (themeInfo.IsDark)
            {
                theme.SetDarkTheme();
            }
            else
            {
                theme.SetLightTheme();
            }

            paletteHelper.SetTheme(theme);
        }
    }
}
