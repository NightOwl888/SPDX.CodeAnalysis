using System;

namespace SPDX.CodeAnalysis.Tests
{
    internal static class StringExtensions
    {
        public static string[] SplitLines(this string str) =>
            str.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    }
}
