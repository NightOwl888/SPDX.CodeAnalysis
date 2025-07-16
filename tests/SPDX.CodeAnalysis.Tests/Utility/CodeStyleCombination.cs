// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using SPDX.CodeAnalysis.Tests.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    public sealed class CodeStyleCombination
    {
        public NamespaceStyle NamespaceStyle { get; }
        public CodeStyleSlot LicenseIdentifierSlot { get; }
        public CodeStyleSlot FileCopyrightTextSlot { get; }
        public CodeStyleSlot LicenseHeaderTextSlot { get; }

        public IEnumerable<CodeStyleSlot> Slots => new[] { LicenseIdentifierSlot, FileCopyrightTextSlot, LicenseHeaderTextSlot };

        public CodeStyleCombination(
            NamespaceStyle namespaceStyle,
            CodeStyleSlot licenseIdentifierSlot,
            CodeStyleSlot copyrightTextSlot,
            CodeStyleSlot licenseHeaderTextSlot)
        {
            NamespaceStyle = namespaceStyle;
            LicenseIdentifierSlot = licenseIdentifierSlot;
            FileCopyrightTextSlot = copyrightTextSlot;
            LicenseHeaderTextSlot = licenseHeaderTextSlot;
        }

        public override string ToString()
            => $"{NamespaceStyle} | {LicenseIdentifierSlot}, {FileCopyrightTextSlot}, {LicenseHeaderTextSlot}";

        public static IEnumerable<CodeStyleCombination> AllFor(CodeLanguage language)
        {
            var namespaceStyles = language switch
            {
                CodeLanguage.CSharp => new[] { NamespaceStyle.BlockScoped, NamespaceStyle.FileScoped },
                CodeLanguage.VisualBasic => new[] { NamespaceStyle.BlockScoped },
                _ => throw new NotSupportedException()
            };

            foreach (var nsStyle in namespaceStyles)
                foreach (var id in CodeStyleSlot.AllFor(CodeStyleElement.LicenseIdentifier))
                    foreach (var copy in CodeStyleSlot.AllFor(CodeStyleElement.FileCopyrightText))
                        foreach (var lic in CodeStyleSlot.AllFor(CodeStyleElement.LicenseHeaderText))
                            yield return new CodeStyleCombination(nsStyle, id, copy, lic);
        }
    }

    //public sealed class CodeStyleCombination
    //{
    //    public required CodeLanguage Language { get; init; }
    //    public required NamespaceStyle NamespaceStyle { get; init; }
    //    public required CommentStyle CommentStyle { get; init; }
    //    public required FilePosition Position { get; init; }

    //    public string DefaultSourceFilePath => Language switch
    //    {
    //        CodeLanguage.CSharp => "project/src/specialized/stuff/foo.cs",
    //        CodeLanguage.VisualBasic => "project/src/specialized/stuff/foo.vb",
    //        _ => throw new NotSupportedException()
    //    };

    //    public string GenerateTestCode(
    //        LicenseComponent components,
    //        string? licenseIdentifierText,
    //        string? copyrightText,
    //        string? licenseText)
    //    {
    //        IFileBuilder builder = Language switch
    //        {
    //            CodeLanguage.CSharp => new CSharpFileBuilderAdapter(NamespaceStyle),
    //            // TODO: Add Visual Basic implementation when ready
    //            _ => throw new NotSupportedException()
    //        };

    //        if (components.HasFlag(LicenseComponent.LicenseIdentifier))
    //        {
    //            builder.WriteComment(Position, licenseIdentifierText ?? "SPDX-License-Identifier: " + Constants.License.Apache2.SPDX_LicenseIdentifier, CommentStyle);
    //        }

    //        if (components.HasFlag(LicenseComponent.CopyrightText))
    //        {
    //            builder.WriteComment(Position, copyrightText ?? "SPDX-FileCopyrightText: " + Constants.License.Apache2.SPDX_FileCopyrightText, CommentStyle);
    //        }

    //        if (components.HasFlag(LicenseComponent.LicenseText))
    //        {
    //            builder.WriteComment(Position, licenseText ?? Constants.License.Apache2.Header, CommentStyle);
    //        }

    //        return builder.ToString();
    //    }

    //    public static IEnumerable<CodeStyleCombination> AllFor(CodeLanguage language)
    //    {
    //        var positions = new[]
    //        {
    //            FilePosition.BeforeUsings,
    //            FilePosition.BeforeNamespace,
    //            FilePosition.BeforeType
    //        };

    //        var commentStyles = new[]
    //        {
    //            CommentStyle.SingleLine,
    //            CommentStyle.MultiLine
    //        };

    //        var namespaceStyles = language switch
    //        {
    //            CodeLanguage.CSharp => new[] {
    //                NamespaceStyle.Global,
    //                NamespaceStyle.FileScoped,
    //                NamespaceStyle.BlockScoped
    //            },
    //            CodeLanguage.VisualBasic => new[] {
    //                NamespaceStyle.BlockScoped // VB only supports block style
    //            },
    //            _ => throw new NotSupportedException()
    //        };

    //        foreach (var nsStyle in namespaceStyles)
    //            foreach (var cmStyle in commentStyles)
    //                foreach (var position in positions)
    //                {
    //                    yield return new CodeStyleCombination
    //                    {
    //                        Language = language,
    //                        NamespaceStyle = nsStyle,
    //                        CommentStyle = cmStyle,
    //                        Position = position
    //                    };
    //                }
    //    }

    //    public override string ToString()
    //    {
    //        return $"{Language}-{NamespaceStyle}-{CommentStyle}-{Position}";
    //    }
    //}
}
