// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommandLine;

namespace GitTimelapseView
{
    public class StartupOptions
    {
        [Option('i', "input", Required = false, HelpText = "Input file to be opened")]
        public string? InputFile { get; set; }

        [Option('l', "line", Required = false, HelpText = "Line number to select")]
        public int? LineNumber { get; set; }

        [Option('r', "rev", Required = false, HelpText = "Revision index to select")]
        public int? RevisionIndex { get; set; }

        [Option("sha", Required = false, HelpText = "Sha of the revision to select")]
        public string? Sha { get; set; }
    }
}
