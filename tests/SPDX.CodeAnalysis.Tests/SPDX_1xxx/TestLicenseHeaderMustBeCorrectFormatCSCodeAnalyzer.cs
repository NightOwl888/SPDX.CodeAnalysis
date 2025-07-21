// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SPDX.CodeAnalysis.Tests.CSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    public partial class TestLicenseHeaderMustBeCorrectFormatCSCodeAnalyzer : TestLicenseHeaderMustBeCorrectFormat
    {
        private static readonly FileSystemXml fileSystemXml = new CSharpFileSystemXml();
        private static CodeStyleCombination[] Styles = CodeStyleCombination.AllFor(CodeLanguage.CSharp).ToArray();

        public override CodeLanguage Language => CodeLanguage.CSharp;

        public override FileSystemXml FileSystemXml => fileSystemXml;

        [Test]
        public async Task SPDX_1005_LicenseTextMustExist_Apache2_SingleMatch_Exists()
        {
            string testCodeFilePath = "project/src/baz.cs";
            string testCode = CSharpFileBuilder.Create(NamespaceStyle.BlockScoped)
                    .WithCommentBeforeUsings("SPDX-License-Identifier: Apache-2.0")
                    .WithCommentBeforeUsings("SPDX-FileCopyrightText: Copyright 2025-2028 John Smith")
                    .WithCommentBeforeUsings("")
                    .WithCommentBeforeUsings(Constants.License.Apache2.Header)
                    .ToString();

            //Console.Write(testCode); // Enable for debugging


            await RunTestAsync(
                FileSystemXml.With2OverriddenLevels,
                testCode,
                testCodeFilePath,
                enabledDiagnostics: new[]
                {
                    Descriptors.SPDX_1005_LicenseTextMustExist.Id
                },
                expectedDiagnostics: NoDiagnosticResults
            );
        }


        [Test]
        public async Task SPDX_1006_NoLicenseHeaderTextConfiguration()
        {
            string testCodeFilePath = "project/src/baz.cs";
            string testCode = CSharpFileBuilder.Create(NamespaceStyle.BlockScoped)
                    .WithCommentBeforeUsings("SPDX-License-Identifier: Apache-2.0")
                    .WithCommentBeforeUsings("SPDX-FileCopyrightText: Copyright 2025-2028 John Smith")
                    .WithCommentBeforeUsings(Constants.License.Apache2.Header)
                    .ToString();

            await RunTestAsync(
                FileSystemXml.NoConfiguration,
                testCode,
                testCodeFilePath,
                enabledDiagnostics: new[]
                {
                    Descriptors.SPDX_1006_NoLicenseHeaderTextConfiguration.Id
                },
                expectedDiagnostics: new[] {
                    DiagnosticResult
                        .CompilerWarning(Descriptors.SPDX_1006_NoLicenseHeaderTextConfiguration.Id)
                        .WithMessage(FormatMessage(Descriptors.SPDX_1006_NoLicenseHeaderTextConfiguration.MessageFormat))
                }
            );
        }

        [Test]
        public async Task EmptyFile()
        {
            string testCodeFilePath = "project/src/baz.cs";
            string expectedTestCodeFilePath = NormalizePath(testCodeFilePath);

            await RunTestAsync(
                FileSystemXml.Basic,
                testCode: "",
                testCodeFilePath,
                expectedDiagnostics: new[] {
                    DiagnosticResult
                        .CompilerWarning(Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id)
                        .WithSpan(expectedTestCodeFilePath, 1, 1, 1, 1)
                        .WithMessage(FormatMessage(Descriptors.SPDX_1000_LicenseIdentifierMustExist.MessageFormat, LicenseIdentifierTag)),
                    DiagnosticResult
                        .CompilerWarning(Descriptors.SPDX_1002_FileCopyrightTextMustExist.Id)
                        .WithSpan(expectedTestCodeFilePath, 1, 1, 1, 1)
                        .WithMessage(FormatMessage(Descriptors.SPDX_1002_FileCopyrightTextMustExist.MessageFormat, FileCopyrightTextTag)),
                }
            );
        }



        [Test]
        public async Task TestDiagnostic_WithCode_WithoutAnyLicenseInfo()
        {
            string testCode = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

public class MyClass
{
    public void MyMethod()
    {
    }
    public void MyMethod(int n)
    {
    }
    protected internal bool LessThan(float termA, float termB)
    {
        return termA < termB;
    }
}
";

            string testCodeFilePath = "project/src/baz.cs";
            string expectedTestCodeFilePath = NormalizePath(testCodeFilePath);
            var test = new LicenseHeaderMustBeCorrectFormatTestDriver(FileSystemXml.With1OverriddenLevel, CodeLanguage.CSharp)
            {
                TestCode = testCode,
                TestCodeFilePath = testCodeFilePath,
            };

            //test.EnabledDiagnostics.AddRange(new[]
            //{
            //    Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
            //    Descriptors.SPDX_1002_LicenseCopyrightTextMustExist.Id
            //});

            test.ExpectedDiagnostics.AddRange(new[]
            {
                DiagnosticResult
                    .CompilerWarning(Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id)
                    .WithSpan(expectedTestCodeFilePath, 1, 1, 1, 1)
                    .WithMessage(FormatMessage(Descriptors.SPDX_1000_LicenseIdentifierMustExist.MessageFormat, LicenseIdentifierTag)),
                DiagnosticResult
                    .CompilerWarning(Descriptors.SPDX_1002_FileCopyrightTextMustExist.Id)
                    .WithSpan(expectedTestCodeFilePath, 1, 1, 1, 1)
                    .WithMessage(FormatMessage(Descriptors.SPDX_1002_FileCopyrightTextMustExist.MessageFormat, FileCopyrightTextTag)),
            });

            await test.RunAsync();
        }
    }
}
