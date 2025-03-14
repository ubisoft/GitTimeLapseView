// Copyright (c) Ubisoft. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.CompilerServices;
using GitTimelapseView.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace GitTimelapseView.Tests
{
    [TestFixture]
    public class TimelapseHistoryTests
    {
        [Test]
        public void TimelapseHistory_ForThisSourceFile_ContainsAtLeastTwoCommits()
        {
            var sourceDirectoryPath = Path.GetDirectoryName(GetThisSourceFilePath());
            Assert.That(sourceDirectoryPath, Is.Not.Null);
            if (sourceDirectoryPath == null)
            {
                return;
            }

            var readmePath = Path.Combine(sourceDirectoryPath, "..", "README.md");
            var history = new FileHistory(readmePath);
            history.Initialize(NullLogger.Instance);

            Assert.That(history, Is.Not.Null);
            Assert.That(history.Revisions.Count, Is.Not.Null);
            Assert.That(history.Revisions.Count, Is.GreaterThanOrEqualTo(1));
        }

        private static string? GetThisSourceFilePath([CallerFilePath] string? srcFilePath = null) => srcFilePath;
    }
}
