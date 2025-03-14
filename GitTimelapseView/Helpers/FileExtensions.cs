// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace GitTimelapseView.Helpers;

public static class FileExtensions
{
    static FileExtensions()
    {
        ExtensionsToLanguage = new Dictionary<string, string>
        {
            { ".js", "javascript" },
            { ".ts", "typescript" },
            { ".cs", "csharp" },
            { ".vb", "vb" },
            { ".html", "html" },
            { ".htm", "html" },
            { ".css", "css" },
            { ".scss", "scss" },
            { ".json", "json" },
            { ".xml", "xml" },
            { ".yaml", "yaml" },
            { ".yml", "yaml" },
            { ".md", "markdown" },
            { ".py", "python" },
            { ".java", "java" },
            { ".cpp", "cpp" },
            { ".c", "c" },
            { ".h", "c" },
            { ".hpp", "cpp" },
            { ".cc", "cpp" },
            { ".go", "go" },
            { ".rs", "rust" },
            { ".swift", "swift" },
            { ".kt", "kotlin" },
            { ".kts", "kotlin" },
            { ".php", "php" },
            { ".rb", "ruby" },
            { ".pl", "perl" },
            { ".sh", "shell" },
            { ".bat", "bat" },
            { ".ps1", "powershell" },
            { ".lua", "lua" },
            { ".r", "r" },
            { ".dart", "dart" },
            { ".sql", "sql" },
            { ".graphql", "graphql" },
            { ".ini", "ini" },
            { ".toml", "toml" },
            { ".cfg", "ini" },
            { ".tex", "latex" },
            { ".jsx", "javascript" },
            { ".tsx", "typescript" },
            { ".svelte", "svelte" },
            { ".vue", "vue" },
            { ".razor", "razor" },
            { ".aspx", "razor" },
            { ".cshtml", "razor" },
            { ".handlebars", "handlebars" },
            { ".hbs", "handlebars" },
            { ".ejs", "html" },
        };
    }

    public static IDictionary<string, string> ExtensionsToLanguage { get; }
}
