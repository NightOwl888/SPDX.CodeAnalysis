using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    public class CSharpFileBuilder
    {
        private readonly List<string> _beforeUsings = new();
        private readonly List<string> _beforeNamespace = new();
        private readonly List<string> _beforeType = new();

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

        public CSharpFileBuilder BeforeUsings(string content) => AddLines(_beforeUsings, content);
        public CSharpFileBuilder BeforeNamespace(string content) => AddLines(_beforeNamespace, content);
        public CSharpFileBuilder BeforeType(string content) => AddLines(_beforeType, content);

        private CSharpFileBuilder AddLines(List<string> list, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return this; // only used inside method chain
            list.AddRange(content
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Where(line => !string.IsNullOrWhiteSpace(line)));
            return this;
        }

        private static string IndentLines(IList<string> lines, bool indent)
        {
            if (lines.Count == 0) return string.Empty;

            var transformedLines = indent ? lines.Select(line => Indent + line) : lines;
            return string.Join(Environment.NewLine, transformedLines);
        }

        public override string ToString()
        {
            string beforeUsings = string.Join(Environment.NewLine, _beforeUsings);
            string beforeNamespace = string.Join(Environment.NewLine, _beforeNamespace);
            string beforeType = IndentLines(_beforeType, _style == NamespaceStyle.BlockScoped);

            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(beforeUsings))
            {
                sb.AppendLine(beforeUsings);
                sb.AppendLine();
            }

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Text;");

            if (!string.IsNullOrWhiteSpace(beforeNamespace))
            {
                sb.AppendLine();
                sb.AppendLine(beforeNamespace);
            }

            switch (_style)
            {
                case NamespaceStyle.Global:
                    sb.AppendLine();
                    if (!string.IsNullOrWhiteSpace(beforeType))
                        sb.AppendLine(beforeType);
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
                    if (!string.IsNullOrWhiteSpace(beforeType))
                        sb.AppendLine(beforeType);
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
                    if (!string.IsNullOrWhiteSpace(beforeType))
                        sb.AppendLine(beforeType);
                    if (_includeType)
                    {
                        sb.AppendLine();
                        sb.Append(IndentLines(GlobalTypeDeclaration().SplitLines(), true));
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
