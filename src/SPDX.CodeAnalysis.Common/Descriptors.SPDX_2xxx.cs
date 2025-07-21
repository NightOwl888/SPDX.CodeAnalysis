// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static SPDX.CodeAnalysis.Category;

namespace SPDX.CodeAnalysis
{
    public static partial class Descriptors
    {
        public static DiagnosticDescriptor SPDX_2000_NoLicenseHeaderTextConfiguration { get; } =
            Diagnostic(
                "SPDX_2000",
                Configuration,
                Warning
            );
    }
}
