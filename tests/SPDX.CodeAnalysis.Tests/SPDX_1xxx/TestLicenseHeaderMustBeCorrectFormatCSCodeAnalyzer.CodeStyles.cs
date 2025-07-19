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
    }
}
