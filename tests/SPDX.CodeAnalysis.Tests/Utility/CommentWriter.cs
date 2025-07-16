// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.IO;
using System.Text;
using static SPDX.CodeAnalysis.MemoryExtensions;

namespace SPDX.CodeAnalysis.Tests
{
    public static class CommentWriter
    {
        public static void AppendComment(this StringBuilder sb, ReadOnlySpan<char> content,
            CodeLanguage language = CodeLanguage.CSharp,
            CommentStyle style = CommentStyle.SingleLine,
            int indentLevel = 0,
            int indentChars = 4)
        {
            using TextWriter writer = new StringWriter(sb);
            WriteComment(writer, content, language, style, indentLevel, indentChars);
        }

        public static void AppendComment(this StringBuilder sb, ref LineSplitEnumerator lines,
            CodeLanguage language = CodeLanguage.CSharp,
            CommentStyle style = CommentStyle.SingleLine,
            int indentLevel = 0,
            int indentChars = 4)
        {
            using TextWriter writer = new StringWriter(sb);
            WriteComment(writer, ref lines, language, style, indentLevel, indentChars);
        }

        public static void AppendComment(this StringBuilder sb, ReadOnlySpan<string> lines,
            CodeLanguage language = CodeLanguage.CSharp,
            CommentStyle style = CommentStyle.SingleLine,
            int indentLevel = 0,
            int indentChars = 4)
        {
            using TextWriter writer = new StringWriter(sb);
            WriteComment(writer, lines, language, style, indentLevel, indentChars);
        }

        public static void WriteComment(this TextWriter writer, ReadOnlySpan<char> content,
            CodeLanguage language = CodeLanguage.CSharp,
            CommentStyle style = CommentStyle.SingleLine,
            int indentLevel = 0,
            int indentChars = 4)
        {
            LineSplitEnumerator lines = content.SplitLines();
            WriteComment(writer, ref lines, language, style, indentLevel, indentChars);
        }

        public static void WriteComment(
            this TextWriter writer,
            ref LineSplitEnumerator lines,
            CodeLanguage language = CodeLanguage.CSharp,
            CommentStyle style = CommentStyle.SingleLine,
            int indentLevel = 0,
            int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            switch (language)
            {
                case CodeLanguage.CSharp:

                    if (style == CommentStyle.SingleLine)
                    {
                        foreach (var line in lines)
                        {
                            writer.Write(indent); writer.Write("// "); writer.WriteLine(line.Line.TrimEnd());
                        }
                    }
                    else
                    {
                        writer.Write(indent); writer.WriteLine("/*");
                        foreach (var line in lines)
                        {
                            writer.Write(indent); writer.Write(" *  "); writer.WriteLine(line.Line.TrimEnd());
                        }
                        writer.Write(indent); writer.WriteLine(" */");
                    }
                    break;
                case CodeLanguage.VisualBasic:
                    // NOTE: Style doesn't apply in VB
                    foreach (var line in lines)
                    {
                        writer.Write(indent); writer.Write("' "); writer.WriteLine(line.Line.TrimEnd());
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(language));
            }
        }

        public static void WriteComment(
            this TextWriter writer,
            ReadOnlySpan<string> lines,
            CodeLanguage language = CodeLanguage.CSharp,
            CommentStyle style = CommentStyle.SingleLine,
            int indentLevel = 0,
            int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            switch (language)
            {
                case CodeLanguage.CSharp:

                    if (style == CommentStyle.SingleLine)
                    {
                        foreach (var line in lines)
                        {
                            writer.Write(indent); writer.Write("// "); writer.WriteLine(line.AsSpan().TrimEnd());
                        }
                    }
                    else
                    {
                        writer.Write(indent); writer.WriteLine("/*");
                        foreach (var line in lines)
                        {
                            writer.Write(indent); writer.Write(" *  "); writer.WriteLine(line.AsSpan().TrimEnd());
                        }
                        writer.Write(indent); writer.WriteLine(" */");
                    }
                    break;
                case CodeLanguage.VisualBasic:
                    // NOTE: Style doesn't apply in VB
                    foreach (var line in lines)
                    {
                        writer.Write(indent); writer.Write("' "); writer.WriteLine(line.AsSpan().TrimEnd());
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(language));
            }
        }

        public static void AppendIndented(this StringBuilder sb, ReadOnlySpan<char> content, int indentLevel = 1, int indentChars = 4)
        {
            LineSplitEnumerator lines = content.SplitLines();
            AppendIndented(sb, ref lines, indentLevel, indentChars);
        }

        public static void AppendIndented(this StringBuilder sb, ref LineSplitEnumerator lines, int indentLevel = 1, int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            foreach (var line in lines)
            {
                sb.Append(indent); sb.Append(line);
            }
        }

        public static void AppendIndented(this StringBuilder sb, ReadOnlySpan<string> lines, int indentLevel = 1, int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            foreach (var line in lines)
            {
                sb.Append(indent); sb.Append(line);
            }
        }

        public static void WriteIndented(this TextWriter writer, ReadOnlySpan<char> content, int indentLevel = 1, int indentChars = 4)
        {
            LineSplitEnumerator lines = content.SplitLines();
            WriteIndented(writer, ref lines, indentLevel, indentChars);
        }

        public static void WriteIndented(this TextWriter writer, ref LineSplitEnumerator lines, int indentLevel = 1, int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            foreach (var line in lines)
            {
                writer.Write(indent); writer.Write(line);
            }
        }

        public static void WriteIndented(this TextWriter writer, ReadOnlySpan<string> lines, int indentLevel = 1, int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            foreach (var line in lines)
            {
                writer.Write(indent); writer.Write(line);
            }
        }

        public static void AppendIndentedLine(this StringBuilder sb, ReadOnlySpan<char> content, int indentLevel = 1, int indentChars = 4)
        {
            LineSplitEnumerator lines = content.SplitLines();
            AppendIndentedLine(sb, ref lines, indentLevel, indentChars);
        }

        public static void AppendIndentedLine(this StringBuilder sb, ref LineSplitEnumerator lines, int indentLevel = 1, int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            foreach (var line in lines)
            {
                sb.Append(indent); sb.Append(line); sb.AppendLine();
            }
        }

        public static void AppendIndentedLine(this StringBuilder sb, ReadOnlySpan<string> lines, int indentLevel = 1, int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            foreach (var line in lines)
            {
                sb.Append(indent); sb.Append(line); sb.AppendLine();
            }
        }

        public static void WriteIndentedLine(this TextWriter writer, ReadOnlySpan<char> content, int indentLevel = 1, int indentChars = 4)
        {
            LineSplitEnumerator lines = content.SplitLines();
            WriteIndentedLine(writer, ref lines, indentLevel, indentChars);
        }

        public static void WriteIndentedLine(this TextWriter writer, ref LineSplitEnumerator lines, int indentLevel = 1, int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            foreach (var line in lines)
            {
                writer.Write(indent); writer.WriteLine(line);
            }
        }

        public static void WriteIndentedLine(this TextWriter writer, ReadOnlySpan<string> lines, int indentLevel = 1, int indentChars = 4)
        {
            Span<char> indent = stackalloc char[indentLevel * indentChars];
            indent.Fill(' ');

            foreach (var line in lines)
            {
                writer.Write(indent); writer.WriteLine(line);
            }
        }
    }
}
