// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;

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
}
