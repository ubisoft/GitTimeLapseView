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
