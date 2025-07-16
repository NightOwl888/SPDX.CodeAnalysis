// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    // TODO: Remove this
    public sealed class DiagnosticTestCase
    {
        public required CodeStyleCombination Style { get; init; }
        public string? LicenseIdentifierText { get; init; }
        public string? CopyrightText { get; init; }
        public string? LicenseText { get; init; }
        public LicenseComponent LicenseComponents { get; init; } = LicenseComponent.None; // TODO: Unclear why this is part of this. This should be defined in the test, not in the runner.
        public bool ExpectDiagnostic { get; init; }
        public DiagnosticSeverity Severity { get; init; } = DiagnosticSeverity.Warning;
        public string DiagnosticId { get; init; }
        public string TestCode => string.Empty; //Style.GenerateTestCode(LicenseComponents, LicenseIdentifierText, CopyrightText, LicenseText);
        public string TestCodeFilePath => string.Empty; // Style.DefaultSourceFilePath;

        public override string ToString()
        {
            //return $"{DiagnosticId} [{Style.Name}] - Components: {LicenseComponents}";
            return $"{DiagnosticId} ({Style})";
        }
    }
}
