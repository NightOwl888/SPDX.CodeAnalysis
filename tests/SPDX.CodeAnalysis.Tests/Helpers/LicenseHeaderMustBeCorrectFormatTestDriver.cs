// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using SPDX.CodeAnalysis.CSharp;

namespace SPDX.CodeAnalysis.Tests
{
    public class LicenseHeaderMustBeCorrectFormatTestDriver : FsmlAnalyzerTest<Verifier>
    {
        private readonly LicenseAnalyzerOptions licenseAnalyzerOptions;

        public LicenseHeaderMustBeCorrectFormatTestDriver(string fsmlXml, CodeLanguage language, bool suppressLocation = false, string topLevelDirectoryName = "LICENSES.HEADERS")
            : base(fsmlXml, language, topLevelDirectoryName)
        {
            this.licenseAnalyzerOptions = new LicenseAnalyzerOptions { SuppressLocation = suppressLocation };

            // Disable the check for #pragma diagnositicId for the current analyzer.
            // This check fails if an analyzer outputs messages by default.
            this.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
        }

        protected override DiagnosticAnalyzer CreateCSharpAnalyzer()
        {
            // Inject dependencies into the analyzer for testing
            return new LicenseHeaderMustBeCorrectFormat(new LicenseHeaderCache(), licenseAnalyzerOptions);
        }

        protected override DiagnosticAnalyzer CreateVisualBasicAnalyzer()
        {
            return new EmptyDiagnosticAnalyzer(); // TODO: For now, we don't support VB
        }

    }
}
