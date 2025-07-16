// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static SPDX.CodeAnalysis.Category;

namespace SPDX.CodeAnalysis
{
    public static partial class Descriptors
    {
        public static DiagnosticDescriptor SPDX_1000_LicenseIdentifierMustExist { get; } =
            Diagnostic(
                "SPDX_1000",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX_1001_LicenseIdentifierMustHaveValue { get; } =
            Diagnostic(
                "SPDX_1001",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX_1002_FileCopyrightTextMustExist { get; } =
            Diagnostic(
                "SPDX_1002",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX_1003_FileCopyrightTextMustHaveValue { get; } =
            Diagnostic(
                "SPDX_1003",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX_1004_LicenseCopyrightTextMustPrecedeLicenseIdentifier { get; } =
            Diagnostic(
                "SPDX_1004",
                Licensing,
                Warning
            );

        public static DiagnosticDescriptor SPDX_1005_LicenseTextMustExist { get; } =
            Diagnostic(
                "SPDX_1005",
                Licensing,
                Warning
            );
    }
}
