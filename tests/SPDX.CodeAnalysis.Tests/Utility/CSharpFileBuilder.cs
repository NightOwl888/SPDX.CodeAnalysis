using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SPDX.CodeAnalysis.Tests
{
    public class CSharpFileBuilder
    {
        private class TextLine
        {
            public string Line { get; set; } = string.Empty;
            public bool IsComment { get; set; }
            public CommentStyle CommentStyle { get; set; }
        }

        private readonly List<TextLine> _beforeUsings = new();
        private readonly List<TextLine> _beforeNamespace = new();
        private readonly List<TextLine> _beforeType = new();

        private NamespaceStyle _style;
        private bool _includeType = true;

        private const string Indent = "    ";

        private CSharpFileBuilder(NamespaceStyle style)
        {
            _style = style;
        }

        public static CSharpFileBuilder Create(NamespaceStyle style) => new(style);

        public CSharpFileBuilder WithoutType()
        {
            _includeType = false;
            return this;
        }

        public CSharpFileBuilder BeforeUsings(string content) => AddLines(_beforeUsings, content, false);
        public CSharpFileBuilder BeforeNamespace(string content) => AddLines(_beforeNamespace, content, false);
        public CSharpFileBuilder BeforeType(string content) => AddLines(_beforeType, content, false);

        public CSharpFileBuilder Write(FilePosition position, string content)
        {
            List<TextLine> destination = position switch
            {
                FilePosition.BeforeUsings => _beforeUsings,
                FilePosition.BeforeNamespace => _beforeNamespace,
                FilePosition.BeforeType => _beforeType,
                _ => throw new ArgumentOutOfRangeException(nameof(position))
            };

            AddLines(destination, content, false);
            return this;
        }

        public CSharpFileBuilder WriteComment(FilePosition position, string content, CommentStyle style = CommentStyle.SingleLine)
        {
            List<TextLine> destination = position switch
            {
                FilePosition.BeforeUsings => _beforeUsings,
                FilePosition.BeforeNamespace => _beforeNamespace,
                FilePosition.BeforeType => _beforeType,
                _ => throw new ArgumentOutOfRangeException(nameof(position))
            };

            AddLines(destination, content, true, style);
            return this;
        }

        public CSharpFileBuilder WithCommentBeforeUsings(string content, CommentStyle style = CommentStyle.SingleLine)
            => WriteComment(FilePosition.BeforeUsings, content, style);

        public CSharpFileBuilder WithCommentBeforeNamespace(string content, CommentStyle style = CommentStyle.SingleLine)
            => WriteComment(FilePosition.BeforeNamespace, content, style);

        public CSharpFileBuilder WithCommentBeforeType(string content, CommentStyle style = CommentStyle.SingleLine)
            => WriteComment(FilePosition.BeforeType, content, style);

        private CSharpFileBuilder AddLines(List<TextLine> list, string content, bool isComment, CommentStyle style = CommentStyle.SingleLine)
        {
            if (string.IsNullOrEmpty(content))
                return this;

            foreach (var line in content.SplitLines())
            {
                list.Add(new TextLine { Line = line.Line.ToString(), IsComment = isComment, CommentStyle = style });
            }
            return this;
        }

        private static void RenderTextLines(StringBuilder sb, List<TextLine> lines, int indentLevel = 0, int indentChars = 4)
        {
            int i = 0;
            while (i < lines.Count)
            {
                if (lines[i].IsComment)
                {
                    var commentStyle = lines[i].CommentStyle;
                    var block = new List<string>();
                    while (i < lines.Count && lines[i].IsComment && lines[i].CommentStyle == commentStyle)
                    {
                        block.Add(lines[i].Line);
                        i++;
                    }
                    sb.AppendComment(CollectionsMarshal.AsSpan(block), CodeLanguage.CSharp, commentStyle, indentLevel, indentChars);
                }
                else
                {
                    sb.AppendIndented(lines[i].Line, indentLevel, indentChars);
                    sb.AppendLine();
                    i++;
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (_beforeUsings.Count > 0)
            {
                RenderTextLines(sb, _beforeUsings);
                sb.AppendLine();
            }

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Text;");

            if (_beforeNamespace.Count > 0)
            {
                sb.AppendLine();
                RenderTextLines(sb, _beforeNamespace);
            }

            switch (_style)
            {
                case NamespaceStyle.Global:
                    sb.AppendLine();
                    if (_beforeType.Count > 0)
                        RenderTextLines(sb, _beforeType);
                    if (_includeType)
                    {
                        sb.AppendLine();
                        sb.AppendLine(GlobalTypeDeclaration());
                    }
                    break;

                case NamespaceStyle.FileScoped:
                    sb.AppendLine();
                    sb.AppendLine("namespace MyNamespace;");
                    sb.AppendLine();
                    if (_beforeType.Count > 0)
                        RenderTextLines(sb, _beforeType);
                    if (_includeType)
                    {
                        sb.AppendLine();
                        sb.AppendLine(GlobalTypeDeclaration());
                    }
                    break;

                case NamespaceStyle.BlockScoped:
                    sb.AppendLine();
                    sb.AppendLine("namespace MyNamespace");
                    sb.AppendLine("{");
                    if (_beforeType.Count > 0)
                        RenderTextLines(sb, _beforeType, indentLevel: 1);
                    if (_includeType)
                    {
                        sb.AppendLine();
                        sb.AppendIndentedLine(GlobalTypeDeclaration(), indentLevel: 1);
                    }
                    sb.AppendLine("}");
                    break;
            }

            return sb.ToString();
        }

        private static string GlobalTypeDeclaration() => @"
public class MyClass
{
    public void MyMethod()
    {
    }
}
".Trim();
    }
}
