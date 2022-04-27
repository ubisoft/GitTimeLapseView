// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace GitTimelapseView.Services
{
    public record ThemeInfo
    {
        public ThemeInfo(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string MonacoTheme { get; set; } = string.Empty;

        public bool IsDark { get; set; }
    }
}
