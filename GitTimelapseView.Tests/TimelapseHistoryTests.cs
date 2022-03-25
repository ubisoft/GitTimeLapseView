using System.IO;
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
            Assert.IsNotNull(sourceDirectoryPath);
            if (sourceDirectoryPath == null)
            {
                return;
            }

            var readmePath = Path.Combine(sourceDirectoryPath, "..", "README.md");
            var history = new FileHistory(readmePath);
            history.Initialize(NullLogger.Instance);

            Assert.NotNull(history);
            Assert.GreaterOrEqual(history.Revisions.Count, 4);
        }

        private static string? GetThisSourceFilePath([CallerFilePath] string? srcFilePath = null) => srcFilePath;
    }
}
