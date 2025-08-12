// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static SPDX.CodeAnalysis.Category;

namespace SPDX.CodeAnalysis
{
    public static partial class Descriptors
    {
        public static DiagnosticDescriptor SPDX1000_LicenseIdentifierMustExist { get; } =
            Diagnostic(
                "SPDX1000",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX1001_LicenseIdentifierMustHaveValue { get; } =
            Diagnostic(
                "SPDX1001",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX1002_FileCopyrightTextMustExist { get; } =
            Diagnostic(
                "SPDX1002",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX1003_FileCopyrightTextMustHaveValue { get; } =
            Diagnostic(
                "SPDX1003",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX1004_LicenseCopyrightTextMustPrecedeLicenseIdentifier { get; } =
            Diagnostic(
                "SPDX1004",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX1005_LicenseTextMustExist { get; } =
            Diagnostic(
                "SPDX1005",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX1006_LicenseTextMatchingConfigurationMustMatchAllLines { get; } =
            Diagnostic(
                "SPDX1006",
                Licensing,
                Warning
            );
    }
}
