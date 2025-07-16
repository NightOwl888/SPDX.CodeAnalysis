// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using NUnit.Framework;
using SPDX.CodeAnalysis.Tests.CSharp;
using System;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis.Tests
{
    public partial class TestLicenseHeaderMustBeCorrectFormatCSCodeAnalyzer
    {
        [TestFixtureSource(nameof(AllCombinations))]
        public class CodeStyles : TestLicenseHeaderMustBeCorrectFormatCodeStyles
        {
            private static readonly FileSystemXml fileSystemXml = new CSharpFileSystemXml();

            public static IEnumerable<CodeStyleCombination> AllCombinations =>
                CodeStyleCombination.AllFor(CodeLanguage.CSharp);

            public override CodeLanguage Language => CodeLanguage.CSharp;

            public override FileSystemXml FileSystemXml => fileSystemXml;

            public override string CodeFileExtension => ".cs";

            private readonly CodeStyleCombination style;

            public CodeStyles(CodeStyleCombination style)
            {
                this.style = style ?? throw new ArgumentNullException(nameof(style));
            }

            public override string GenerateTestCode(
                string? licenseIdentifier = DefaultLicenseIdentifier,
                string? fileCopyrightText = DefaultFileCopyrightText,
                string? licenseHeaderText = DefaultLicenseHeaderText)
            {
                var builder = CSharpFileBuilder.Create(style.NamespaceStyle);

                if (licenseIdentifier is not null)
                {
                    WriteSlot(builder, licenseIdentifier, style.LicenseIdentifierSlot);
                }

                if (fileCopyrightText is not null)
                {
                    WriteSlot(builder, fileCopyrightText, style.FileCopyrightTextSlot);
                }

                if (licenseHeaderText is not null)
                {
                    WriteSlot(builder, licenseHeaderText, style.LicenseHeaderTextSlot);
                }

                return builder.ToString();

                static void WriteSlot(CSharpFileBuilder builder, string content, CodeStyleSlot slot)
                {
                    builder.WriteComment(slot.Position, content, slot.CommentStyle);
                }
            }
        }

        //[TestFixture(NamespaceStyle.BlockScoped, CommentStyle.MultiLine)]
        //[TestFixture(NamespaceStyle.BlockScoped, CommentStyle.SingleLine)]
        //[TestFixture(NamespaceStyle.FileScoped, CommentStyle.MultiLine)]
        //[TestFixture(NamespaceStyle.FileScoped, CommentStyle.SingleLine)]
        //[TestFixture(NamespaceStyle.Global, CommentStyle.MultiLine)]
        //[TestFixture(NamespaceStyle.Global, CommentStyle.SingleLine)]
        //public class CodeStyles : TestLicenseHeaderMustBeCorrectFormat
        //{
        //    private readonly NamespaceStyle namespaceStyle;
        //    private readonly CommentStyle commentStyle;

        //    public override CodeLanguage Language => CodeLanguage.CSharp;

        //    //private CSharpFileBuilder builder;

        //    public CodeStyles(NamespaceStyle namespaceStyle, CommentStyle commentStyle)
        //    {
        //        this.namespaceStyle = namespaceStyle;
        //        this.commentStyle = commentStyle;
        //        //this.builder = CSharpFileBuilder.Create(namespaceStyle);
        //    }

        //    [Test]
        //    public async Task WholeLicenseHeaderBeforeUsings_ProducesNoDiagnosticResults()
        //    {
        //        string testCode = CSharpFileBuilder.Create(namespaceStyle)
        //                            .WithCommentBeforeUsings("SPDX-License-Identifier: Apache-2.0", commentStyle)
        //                            .WithCommentBeforeUsings("SPDX-FileCopyrightText: Copyright 2025-2028 John Smith", commentStyle)
        //                            .WithCommentBeforeUsings(Constants.License.Apache2.Header, commentStyle)
        //                            .ToString();

        //        string testCodeFilePath = "project/src/baz.cs";
        //        string expectedTestCodeFilePath = NormalizePath(testCodeFilePath);

        //        await RunTestAsync(
        //            FileSystemXml.Basic,
        //            testCode,
        //            testCodeFilePath,
        //            expectedDiagnostics: NoDiagnosticResults
        //        );
        //    }

        //    [Test]
        //    public async Task SPDXTagsBeforeUsings_LicenseHeaderBeforeType_ProducesNoDiagnosticResults()
        //    {
        //        string testCode = CSharpFileBuilder.Create(namespaceStyle)
        //                            .WithCommentBeforeUsings("SPDX-License-Identifier: Apache-2.0", commentStyle)
        //                            .WithCommentBeforeUsings("SPDX-FileCopyrightText: Copyright 2025-2028 John Smith", commentStyle)
        //                            .WithCommentBeforeType(Constants.License.Apache2.Header, commentStyle)
        //                            .ToString();

        //        string testCodeFilePath = "project/src/baz.cs";
        //        string expectedTestCodeFilePath = NormalizePath(testCodeFilePath);

        //        await RunTestAsync(
        //            FileSystemXml.Basic,
        //            testCode,
        //            testCodeFilePath,
        //            expectedDiagnostics: NoDiagnosticResults
        //        );
        //    }

        //    //private class LicenseHeaderElement
        //    //{
        //    //    public enum Type
        //    //    {

        //    //    }
        //    //}


        //    //public static string GenerateSourceCode(
        //    //    NamespaceStyle namespaceStyle,
        //    //    CommentStyle commentStyle,
        //    //    LicenseMissingValues missingValues = LicenseMissingValues.None,
        //    //    LicensePosition licenseIdentifierPosition = LicensePosition.BeforeUsings,
        //    //    LicensePosition fileCopyrightPosition = LicensePosition.BeforeUsings,
        //    //    LicensePosition licenseHeaderTextPosition = LicensePosition.BeforeUsings)
        //    //{
        //    //    var builder = CSharpFileBuilder.Create(namespaceStyle);

        //    //    if (commentStyle == CommentStyle.SingleLine)
        //    //    {
        //    //        string licenseIdentifierText = HasMisingValue(missingValues, LicenseMissingValues.LicenseIdentifier)
        //    //            ? "// SPDX-License-Identifier:"
        //    //            : "// SPDX-License-Identifier: Apache-2.0";
        //    //        string fileCopyrightText = HasMisingValue(missingValues, LicenseMissingValues.FileCopyrightText)
        //    //            ? "// SPDX-FileCopyrightText:"
        //    //            : "// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith";
        //    //        string licenseText = HasMisingValue(missingValues, LicenseMissingValues.LicenseHeaderText)
        //    //            ? string.Empty
        //    //            : ApacheLicenseHeaderTextSingleLineComment;

        //    //        builder.Write(licenseIdentifierPosition, licenseIdentifierText);
        //    //        builder.Write(fileCopyrightPosition, fileCopyrightText);
        //    //        if (!string.IsNullOrEmpty(licenseText))
        //    //        {
        //    //            builder.Write(licenseHeaderTextPosition, licenseText);
        //    //        }
        //    //    }
        //    //    else // CommentStyle.MultiLine
        //    //    {
        //    //        builder = builder.BeforeUsings("/*");

        //    //        if (HasMisingValue(missingValues, LicenseMissingValues.LicenseIdentifier))
        //    //        {
        //    //            builder = builder.BeforeUsings(" * SPDX-License-Identifier: Apache-2.0");
        //    //        }
        //    //        else
        //    //        {
        //    //            builder = builder.BeforeUsings(" * SPDX-License-Identifier:");
        //    //        }

        //    //        if (HasMisingValue(missingValues, LicenseMissingValues.FileCopyrightText))
        //    //        {
        //    //            builder = builder.BeforeUsings(" * SPDX-FileCopyrightText: Copyright 2025-2028 John Smith");
        //    //        }
        //    //        else
        //    //        {
        //    //            builder = builder.BeforeUsings(" * SPDX-FileCopyrightText:");
        //    //        }

        //    //        builder = builder.BeforeUsings(" */");

        //    //        if (HasMisingValue(missingValues, LicenseMissingValues.LicenseHeaderText))
        //    //        {
        //    //            builder = builder.BeforeType(ApacheLicenseHeaderTextMultiLineComment);
        //    //        }
        //    //        else
        //    //        {
        //    //            // no-op
        //    //        }
        //    //    }

        //    //    return builder.ToString();
        //    //}


        //    //private void Write_SpdxTagsBeforeUsings_LicenseHeaderBeforeType(LicenseMissingValues licenseParts = LicenseMissingValues.All)
        //    //{
        //    //    if (commentStyle == CommentStyle.SingleLine)
        //    //    {
        //    //        if (HasMisingValue(licenseParts, LicenseMissingValues.LicenseIdentifier))
        //    //        {
        //    //            builder = builder.BeforeUsings("// SPDX-License-Identifier: Apache-2.0");
        //    //        }
        //    //        else
        //    //        {
        //    //            builder = builder.BeforeUsings("// SPDX-License-Identifier:");
        //    //        }

        //    //        if (HasMisingValue(licenseParts, LicenseMissingValues.FileCopyrightText))
        //    //        {
        //    //            builder = builder.BeforeUsings("// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith");
        //    //        }
        //    //        else
        //    //        {
        //    //            builder = builder.BeforeUsings("// SPDX-FileCopyrightText:");
        //    //        }

        //    //        if (HasMisingValue(licenseParts, LicenseMissingValues.FileCopyrightText))
        //    //        {
        //    //            builder = builder.BeforeType(ApacheLicenseHeaderTextSingleLineComment);
        //    //        }
        //    //        else
        //    //        {
        //    //            // no-op
        //    //        }
        //    //    }
        //    //    else // CommentStyle.MultiLine
        //    //    {
        //    //        builder = builder.BeforeUsings("/*");

        //    //        if (HasMisingValue(licenseParts, LicenseMissingValues.LicenseIdentifier))
        //    //        {
        //    //            builder = builder.BeforeUsings(" * SPDX-License-Identifier: Apache-2.0");
        //    //        }
        //    //        else
        //    //        {
        //    //            builder = builder.BeforeUsings(" * SPDX-License-Identifier:");
        //    //        }

        //    //        if (HasMisingValue(licenseParts, LicenseMissingValues.FileCopyrightText))
        //    //        {
        //    //            builder = builder.BeforeUsings(" * SPDX-FileCopyrightText: Copyright 2025-2028 John Smith");
        //    //        }
        //    //        else
        //    //        {
        //    //            builder = builder.BeforeUsings(" * SPDX-FileCopyrightText:");
        //    //        }

        //    //        builder = builder.BeforeUsings(" */");

        //    //        if (HasMisingValue(licenseParts, LicenseMissingValues.FileCopyrightText))
        //    //        {
        //    //            builder = builder.BeforeType(ApacheLicenseHeaderTextMultiLineComment);
        //    //        }
        //    //        else
        //    //        {
        //    //            // no-op
        //    //        }
        //    //    }

        //    //    //return builder.ToString();
        //    //}

        //    //private static string GetLicenseIdentifierString(bool excludeValue, CommentStyle commentStyle)
        //    //{
        //    //    if (commentStyle == CommentStyle.SingleLine)
        //    //        return excludeValue ? "// SPDX-License-Identifier:" : "// SPDX-License-Identifier: Apache-2.0"
        //    //}


        //    //public static bool HasMisingValue(LicenseMissingValues setFlags, LicenseMissingValues checkFor)
        //    //    => (setFlags & checkFor) == checkFor;

        //    //[Flags]
        //    //public enum LicenseMissingValues
        //    //{
        //    //    None = 0,
        //    //    LicenseIdentifier = 1,
        //    //    FileCopyrightText = 2,
        //    //    LicenseHeaderText = 4
        //    //}

        //    //public enum LicensePosition
        //    //{
        //    //    BeforeUsings,
        //    //    BeforeNamespace,
        //    //    BeforeType,
        //    //}

        //    //private bool HasLicensePart(LicenseParts setFlags, LicenseParts checkFor)
        //    //    => (setFlags & checkFor) == checkFor;

        //    //[Flags]
        //    //private enum LicenseParts
        //    //{
        //    //    None = 0,
        //    //    LicenseIdentifier = 1,
        //    //    LicenseIdentifierValue = 2,
        //    //    FileCopyrightText = 4,
        //    //    FileCopyrightTextValue = 8,
        //    //    LicenseHeaderText = 16,
        //    //    All = LicenseIdentifier | LicenseIdentifierValue | FileCopyrightText | FileCopyrightTextValue | LicenseHeaderText
        //    //}
        //}
    }
}
