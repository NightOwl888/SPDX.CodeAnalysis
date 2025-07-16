// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    public abstract class TestLicenseHeaderMustBeCorrectFormatCodeStyles : TestLicenseHeaderMustBeCorrectFormat
    {
        [Test]
        public virtual async Task SPDX_1000_LicenseIdentifierMustExist_Exists_ShouldProduceNoDiagnostic()
        {
            string diagnosticId = Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id;

            await RunTestAsync(
                FileSystemXml.Basic,
                testCode: GenerateTestCode(),
                testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
                enabledDiagnostics: new[] { diagnosticId },
                expectedDiagnostics: NoDiagnosticResults,
                suppressLocation: true);
        }

        [Test]
        public virtual async Task SPDX_1000_LicenseIdentifierMustExist_DoesNotExist_ShouldProduceDiagnositc()
        {
            string diagnosticId = Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id;

            await RunTestAsync(
                FileSystemXml.Basic,
                testCode: GenerateTestCode(licenseIdentifier: null),
                testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
                enabledDiagnostics: new[] { diagnosticId },
                expectedDiagnostics: new[] { DiagnosticResult.CompilerWarning(diagnosticId) },
                suppressLocation: true);
        }

        [Test]
        public virtual async Task SPDX_1001_LicenseIdentifierMustHaveValue_HasValue_ShouldProduceNoDiagnostic()
        {
            string diagnosticId = Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id;

            await RunTestAsync(
                FileSystemXml.Basic,
                testCode: GenerateTestCode(),
                testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
                enabledDiagnostics: new[] { diagnosticId },
                expectedDiagnostics: NoDiagnosticResults,
                suppressLocation: true);
        }

        [Test]
        public virtual async Task SPDX_1001_LicenseIdentifierMustHaveValue_HasNoValue_ShouldProduceDiagnositc()
        {
            string diagnosticId = Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id;

            await RunTestAsync(
                FileSystemXml.Basic,
                testCode: GenerateTestCode(licenseIdentifier: LicenseIdentifierTag),
                testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
                enabledDiagnostics: new[] { diagnosticId },
                expectedDiagnostics: new[] { DiagnosticResult.CompilerWarning(diagnosticId) },
                suppressLocation: true);
        }

        [Test]
        public virtual async Task SPDX_1002_FileCopyrightTextMustExist_Exists_ShouldProduceNoDiagnostic()
        {
            string diagnosticId = Descriptors.SPDX_1002_FileCopyrightTextMustExist.Id;

            await RunTestAsync(
                FileSystemXml.Basic,
                testCode: GenerateTestCode(),
                testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
                enabledDiagnostics: new[] { diagnosticId },
                expectedDiagnostics: NoDiagnosticResults,
                suppressLocation: true);
        }

        [Test]
        public virtual async Task SPDX_1002_FileCopyrightTextMustExist_DoesNotExist_ShouldProduceDiagnositc()
        {
            string diagnosticId = Descriptors.SPDX_1002_FileCopyrightTextMustExist.Id;

            await RunTestAsync(
                FileSystemXml.Basic,
                testCode: GenerateTestCode(fileCopyrightText: null),
                testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
                enabledDiagnostics: new[] { diagnosticId },
                expectedDiagnostics: new[] { DiagnosticResult.CompilerWarning(diagnosticId) },
                suppressLocation: true);
        }

        [Test]
        public virtual async Task SPDX_1003_FileCopyrightTextMustHaveValue_HasValue_ShouldProduceNoDiagnostic()
        {
            string diagnosticId = Descriptors.SPDX_1003_FileCopyrightTextMustHaveValue.Id;

            await RunTestAsync(
                FileSystemXml.Basic,
                testCode: GenerateTestCode(),
                testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
                enabledDiagnostics: new[] { diagnosticId },
                expectedDiagnostics: NoDiagnosticResults,
                suppressLocation: true);
        }

        [Test]
        public virtual async Task SPDX_1003_FileCopyrightTextMustHaveValue_HasNoValue_ShouldProduceDiagnositc()
        {
            string diagnosticId = Descriptors.SPDX_1003_FileCopyrightTextMustHaveValue.Id;

            await RunTestAsync(
                FileSystemXml.Basic,
                testCode: GenerateTestCode(fileCopyrightText: FileCopyrightTextTag),
                testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
                enabledDiagnostics: new[] { diagnosticId },
                expectedDiagnostics: new[] { DiagnosticResult.CompilerWarning(diagnosticId) },
                suppressLocation: true);
        }

        // TODO: Implement correct tests for all license text behaviors
        //[Test]
        //public virtual async Task SPDX_1005_LicenseTextMustExist_Exists_ShouldProduceNoDiagnostic()
        //{
        //    string diagnosticId = Descriptors.SPDX_1005_LicenseTextMustExist.Id;

        //    await RunTestAsync(
        //        FileSystemXml.Basic,
        //        testCode: GenerateTestCode(),
        //        testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
        //        enabledDiagnostics: new[] { diagnosticId },
        //        expectedDiagnostics: NoDiagnosticResults,
        //        suppressLocation: true);
        //}

        //[Test]
        //public virtual async Task SPDX_1005_LicenseTextMustExist_DoesNotExist_ShouldProduceDiagnositc()
        //{
        //    string diagnosticId = Descriptors.SPDX_1005_LicenseTextMustExist.Id;

        //    await RunTestAsync(
        //        FileSystemXml.Basic,
        //        testCode: GenerateTestCode(licenseHeaderText: null),
        //        testCodeFilePath: GenerateCodeFilePath("project/src/specialized/stuff/"),
        //        enabledDiagnostics: new[] { diagnosticId },
        //        expectedDiagnostics: new[] { DiagnosticResult.CompilerWarning(diagnosticId) },
        //        suppressLocation: true);
        //}


        public abstract string CodeFileExtension { get; }

        public abstract string GenerateTestCode(
                string? licenseIdentifier = DefaultLicenseIdentifier,
                string? fileCopyrightText = DefaultFileCopyrightText,
                string? licenseHeaderText = DefaultLicenseHeaderText);

        public string GenerateCodeFilePath(string directoryPath, string fileName = DefaultTestFileName)
        {
            return Path.Combine(directoryPath, $"fileName{CodeFileExtension}");
        }
    }
}
