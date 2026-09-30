// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using GitTimelapseView.Core.Common;
using NUnit.Framework;

namespace GitTimelapseView.Tests
{
    [TestFixture]
    public class GitHelpersTests
    {
        [Test]
        public void BuildDiffToolArguments_SamePath_QuotesPathAndUsesDoubleDashSeparator()
        {
            var args = GitHelpers.BuildDiffToolArguments("abc123", @"C:\repo\file.txt", @"C:\repo\file.txt");

            Assert.That(args, Is.EqualTo("difftool -y abc123^ abc123 -- \"C:\\repo\\file.txt\""));
        }

        [Test]
        public void BuildDiffToolArguments_DifferentPaths_QuotesBothPathsAndUsesDoubleDashSeparator()
        {
            var args = GitHelpers.BuildDiffToolArguments("abc123", @"C:\repo\old.txt", @"C:\repo\new.txt");

            Assert.That(args, Is.EqualTo("difftool -y abc123^ abc123 -- \"C:\\repo\\old.txt\" \"C:\\repo\\new.txt\""));
        }

        [Test]
        public void BuildDiffToolArguments_FileNameLooksLikeAnOption_IsKeptAsALiteralQuotedPathAfterDoubleDash()
        {
            // Regression test for a crafted filename such as "payload --extcmd=wscript.exe" being
            // interpreted as a git command-line option instead of a literal path.
            var maliciousPath = @"C:\repo\payload --extcmd=wscript.exe";

            var args = GitHelpers.BuildDiffToolArguments("abc123", maliciousPath, maliciousPath);

            // The whole malicious name must stay inside a single quoted token, placed after "--",
            // so git can never parse "--extcmd=..." as a separate, option-like argument.
            Assert.That(args, Is.EqualTo("difftool -y abc123^ abc123 -- \"C:\\repo\\payload --extcmd=wscript.exe\""));
            Assert.That(args, Does.Contain("-- \""), "Paths must be preceded by the '--' end-of-options marker.");
            Assert.That(args.IndexOf("--extcmd", StringComparison.Ordinal), Is.GreaterThan(args.IndexOf("-- \"", StringComparison.Ordinal)),
                "The attacker-controlled '--extcmd' text must appear inside the quoted path, after the '--' separator, not as a standalone option.");
        }

        [Test]
        public void BuildDiffToolArguments_OldPathLooksLikeAnOption_IsKeptAsALiteralQuotedPath()
        {
            var maliciousOldPath = @"C:\repo\payload --extcmd=wscript.exe";
            var newPath = @"C:\repo\payload.txt";

            var args = GitHelpers.BuildDiffToolArguments("abc123", maliciousOldPath, newPath);

            Assert.That(args, Is.EqualTo("difftool -y abc123^ abc123 -- \"C:\\repo\\payload --extcmd=wscript.exe\" \"C:\\repo\\payload.txt\""));
        }
    }
}
