using System;
using System.Collections.Generic;
using System.Text;

namespace GitTimelapseView.Common
{
    internal static class StringExtensions
    {
        internal static string ReplaceFirst(this string text, string search, string replace, StringComparison comparisonType)
        {
            var index = text.IndexOf(search, comparisonType);
            if (index < 0)
            {
                return text;
            }

            return string.Concat(text.AsSpan(0, index), replace, text.AsSpan(index + search.Length));
        }

        internal static string ConcatLines(this IReadOnlyList<string> lines, int startLine, int lineCount)
        {
            var sb = new StringBuilder();
            for (var i = startLine; i < startLine + lineCount; i++)
            {
                if (i >= 0 && i < lines.Count)
                {
                    sb.AppendLine(lines[i]);
                }
            }

            return sb.ToString();
        }
    }
}
