// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SPDX.CodeAnalysis.Tests.CSharp;
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

        // TODO: Remove this
        //private static IEnumerable<DiagnosticTestDefinition> GetAllDefinitions()
        //{
        //    yield return new SPDX_1000_LicenseIdentifierMustExist_Definition();
        //    yield return new SPDX_1001_LicenseIdentifierMustHaveValue_Definition();
        //}

        //public static IEnumerable<TestCaseData<DiagnosticTestCase>> GetTestCases()
        //{
        //    foreach (var definition in GetAllDefinitions())
        //    {
        //        foreach (var style in Styles)
        //        {
        //            foreach (var testCase in definition.GetTestCases(style))
        //            {
        //                yield return new TestCaseData<DiagnosticTestCase>(testCase);
        //            }
        //        }
        //    }
        //}

        //[TestCaseSource(nameof(GetTestCases))]
        //public async Task RunCSharpTestAsync(DiagnosticTestCase testCase)
        //{
        //    var expectedDiagnostics = testCase.ExpectDiagnostic
        //        ? new List<DiagnosticResult> { ToExpected(testCase) }
        //        : NoDiagnosticResults;

        //    await RunTestAsync(
        //        fileSystemXml: FileSystemXml.Basic,
        //        testCode: testCase.TestCode,
        //        testCodeFilePath: testCase.TestCodeFilePath,
        //        enabledDiagnostics: new List<string> { testCase.DiagnosticId },
        //        expectedDiagnostics: expectedDiagnostics,
        //        suppressLocation: true);
        //}

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
                    DiagnosticResult
                        .CompilerWarning(Descriptors.SPDX_1005_LicenseTextMustExist.Id)
                        .WithSpan(expectedTestCodeFilePath, 1, 1, 1, 1)
                        .WithMessage(FormatMessage(Descriptors.SPDX_1005_LicenseTextMustExist.MessageFormat))
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
                DiagnosticResult
                    .CompilerWarning(Descriptors.SPDX_1005_LicenseTextMustExist.Id)
                    .WithSpan(expectedTestCodeFilePath, 1, 1, 1, 1)
                    .WithMessage(FormatMessage(Descriptors.SPDX_1005_LicenseTextMustExist.MessageFormat)),
            });

            await test.RunAsync();
        }
    }

    //    public partial class TestLicenseHeaderMustBeCorrectFormat : DiagnosticVerifier
    //    {
    //        [OneTimeSetUp]
    //        public void OneTimeSetUp()
    //        {
    //            // TODO: Re-implement this interface for testing

    //            //var licenses = new Dictionary<string, string>
    //            //{
    //            //    [ApacheLicenseHeaderId] = ApacheLicenseHeaderText,
    //            //    [MITLicenseHeaderId] = MITLicenseHeaderText,
    //            //};
    //            //var directories = new Dictionary<string, IDictionary<string, string>>
    //            //{
    //            //    ["LICENSES.HEADERS"] = licenses,
    //            //};

    //            //LicenseHeaderProviderLoader.SetProvider(new InMemoryLicenseHeaderProvider(directories));
    //        }



    //        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    //        {
    //            return new LicenseHeaderMustBeCorrectFormat();
    //        }

    //        private const string MITLicenseHeaderId = "MIT";
    //        private const string ApacheLicenseHeaderId = "Apache-2.0";

    //        private const string MITLicenseHeaderText = @"Licensed to the .NET Foundation under one or more agreements.
    //The .NET Foundation licenses this file to you under the MIT license.";

    //        private const string ApacheLicenseHeaderText = @"Licensed under the Apache License, Version 2.0 (the ""License"");
    //you may not use this file except in compliance with the License.
    //You may obtain a copy of the License at

    //    http://www.apache.org/licenses/LICENSE-2.0

    //Unless required by applicable law or agreed to in writing, software
    //distributed under the License is distributed on an ""AS IS"" BASIS,
    //WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    //See the License for the specific language governing permissions and
    //limitations under the License.";

    //        private const string ApacheLicenseHeaderTextSingleLineComment = @"//  Licensed under the Apache License, Version 2.0 (the ""License"");
    ////  you may not use this file except in compliance with the License.
    ////  You may obtain a copy of the License at
    ////
    ////      http://www.apache.org/licenses/LICENSE-2.0
    ////
    ////  Unless required by applicable law or agreed to in writing, software
    ////  distributed under the License is distributed on an ""AS IS"" BASIS,
    ////  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    ////  See the License for the specific language governing permissions and
    ////  limitations under the License.";

    //        private const string ApacheLicenseHeaderTextMultiLineComment = @"/*
    // *  Licensed under the Apache License, Version 2.0 (the ""License"");
    // *  you may not use this file except in compliance with the License.
    // *  You may obtain a copy of the License at
    // *
    // *      http://www.apache.org/licenses/LICENSE-2.0
    // *
    // *  Unless required by applicable law or agreed to in writing, software
    // *  distributed under the License is distributed on an ""AS IS"" BASIS,
    // *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    // *  See the License for the specific language governing permissions and
    // *  limitations under the License.
    // */";

    //        public static IEnumerable<TestCaseData> LicenseIdentifierUnitTestCases
    //        {
    //            get
    //            {
    //                yield return new TestCaseData(
    //                    // description
    //                    "BlockScopedNamespace_WholeLicenseHeaderBeforeUsings",
    //                    // testCode
    //                    CSharpFileBuilder
    //                        .Create(NamespaceStyle.BlockScoped)
    //                        .BeforeUsings("// SPDX-License-Identifier: Apache-2.0")
    //                        .BeforeUsings("// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith")
    //                        .BeforeUsings(ApacheLicenseHeaderTextSingleLineComment)
    //                        .ToString(),
    //                    // diagnosticIdFilter
    //                    null,
    //                    //new[] {
    //                    //    Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                    //    Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    //},
    //                    // expected
    //                    null
    //                    )/*.SetName("BlockScopedNamespace_WholeLicenseHeaderBeforeUsings")*/;

    //                yield return new TestCaseData(
    //                    // description
    //                    "FileScopedNamespace_WholeLicenseHeaderBeforeUsings",
    //                    // testCode
    //                    CSharpFileBuilder
    //                        .Create(NamespaceStyle.FileScoped)
    //                        .BeforeUsings("// SPDX-License-Identifier: Apache-2.0")
    //                        .BeforeUsings("// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith")
    //                        .BeforeUsings(ApacheLicenseHeaderTextSingleLineComment)
    //                        .ToString(),
    //                    // diagnosticIdFilter
    //                    new[] {
    //                        Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                        Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    },
    //                    // expected
    //                    null
    //                    );

    //                yield return new TestCaseData(
    //                    // description
    //                    "GlobalNamespace_WholeLicenseHeaderBeforeUsings",
    //                    // testCode
    //                    CSharpFileBuilder
    //                        .Create(NamespaceStyle.Global)
    //                        .BeforeUsings("// SPDX-License-Identifier: Apache-2.0")
    //                        .BeforeUsings("// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith")
    //                        .BeforeUsings(ApacheLicenseHeaderTextSingleLineComment)
    //                        .ToString(),
    //                    // diagnosticIdFilter
    //                    new[] {
    //                        Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                        Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    },
    //                    // expected
    //                    null
    //                    );

    //                yield return new TestCaseData(
    //                    // description
    //                    "BlockScopedNamespace_LicenseTagsBeforeUsings_LicenseHeaderBeforeType",
    //                    // testCode
    //                    CSharpFileBuilder
    //                        .Create(NamespaceStyle.BlockScoped)
    //                        .BeforeUsings("// SPDX-License-Identifier: Apache-2.0")
    //                        .BeforeUsings("// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith")
    //                        .BeforeType(ApacheLicenseHeaderTextSingleLineComment)
    //                        .ToString(),
    //                    // diagnosticIdFilter
    //                    new[] {
    //                        Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                        Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    },
    //                    // expected
    //                    null
    //                    );

    //                yield return new TestCaseData(
    //                    // description
    //                    "FileScopedNamespace_LicenseTagsBeforeUsings_LicenseHeaderBeforeType",
    //                    // testCode
    //                    CSharpFileBuilder
    //                        .Create(NamespaceStyle.FileScoped)
    //                        .BeforeUsings("// SPDX-License-Identifier: Apache-2.0")
    //                        .BeforeUsings("// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith")
    //                        .BeforeType(ApacheLicenseHeaderTextSingleLineComment)
    //                        .ToString(),
    //                    // diagnosticIdFilter
    //                    new[] {
    //                        Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                        Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    },
    //                    // expected
    //                    null
    //                    );

    //                yield return new TestCaseData(
    //                    // description
    //                    "GlobalNamespace_LicenseTagsBeforeUsings_LicenseHeaderBeforeType",
    //                    // testCode
    //                    CSharpFileBuilder
    //                        .Create(NamespaceStyle.Global)
    //                        .BeforeUsings("// SPDX-License-Identifier: Apache-2.0")
    //                        .BeforeUsings("// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith")
    //                        .BeforeType(ApacheLicenseHeaderTextSingleLineComment)
    //                        .ToString(),
    //                    // diagnosticIdFilter
    //                    new[] {
    //                        Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                        Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    },
    //                    // expected
    //                    null
    //                    );


    //            }
    //        }

    //        [TestCaseSource(nameof(LicenseIdentifierUnitTestCases))]
    //        public void Test(string description, string testCode, string[] diagnosticIdFilter, DiagnosticResult[] expected)
    //        {
    //            VerifyCSharpDiagnostic(testCode, diagnosticIdFilter, expected ?? Array.Empty<DiagnosticResult>());
    //        }


    //        [Test]
    //        public void TestEmptyFile()
    //        {
    //            var test = @"";

    //            var expected = new DiagnosticResult[]
    //            {
    //                new DiagnosticResult
    //                {
    //                    Id = Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                    Message = string.Format("Header comment including '{0}' is required", "SPDX-License-Identifier:"),
    //                    Severity = DiagnosticSeverity.Warning,
    //                    Locations =
    //                        new[]
    //                        {
    //                            new DiagnosticResultLocation("Test0.cs", 1, 1)
    //                        }
    //                },
    //                new DiagnosticResult
    //                {
    //                    Id = Descriptors.SPDX_1002_LicenseCopyrightTextMustExist.Id,
    //                    Message = string.Format("Header comment including '{0}' is required", "SPDX-CopyrightText:"),
    //                    Severity = DiagnosticSeverity.Warning,
    //                    Locations =
    //                        new[]
    //                        {
    //                            new DiagnosticResultLocation("Test0.cs", 1, 1)
    //                        }
    //                },
    //                new DiagnosticResult
    //                {
    //                    Id = Descriptors.SPDX_1005_LicenseTextMustExist.Id,
    //                    Message = "Header comment including license text is required",
    //                    Severity = DiagnosticSeverity.Warning,
    //                    Locations =
    //                        new[]
    //                        {
    //                            new DiagnosticResultLocation("Test0.cs", 1, 1)
    //                        }
    //                },
    //            };

    //            VerifyCSharpDiagnostic(test, expected);
    //        }

    //        [Test]
    //        public void TestDiagnostic_WithCode_WithoutAnyLicenseInfo()
    //        {
    //            var test = @"
    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //public class MyClass
    //{
    //    public void MyMethod()
    //    {
    //    }
    //    public void MyMethod(int n)
    //    {
    //    }
    //    protected internal override bool LessThan(float termA, float termB)
    //    {
    //        return termA < termB;
    //    }
    //}
    //";

    //            var expected = new DiagnosticResult[]
    //            {
    //                new DiagnosticResult
    //                {
    //                    Id = Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                    Message = string.Format("Header comment including '{0}' is required", "SPDX-License-Identifier:"),
    //                    Severity = DiagnosticSeverity.Warning,
    //                    Locations =
    //                        new[]
    //                        {
    //                            new DiagnosticResultLocation("Test0.cs", 1, 1)
    //                        }
    //                },
    //                new DiagnosticResult
    //                {
    //                    Id = Descriptors.SPDX_1002_LicenseCopyrightTextMustExist.Id,
    //                    Message = string.Format("Header comment including '{0}' is required", "SPDX-CopyrightText:"),
    //                    Severity = DiagnosticSeverity.Warning,
    //                    Locations =
    //                        new[]
    //                        {
    //                            new DiagnosticResultLocation("Test0.cs", 1, 1)
    //                        }
    //                },
    //                new DiagnosticResult
    //                {
    //                    Id = Descriptors.SPDX_1005_LicenseTextMustExist.Id,
    //                    Message = "Header comment including license text is required",
    //                    Severity = DiagnosticSeverity.Warning,
    //                    Locations =
    //                        new[]
    //                        {
    //                            new DiagnosticResultLocation("Test0.cs", 1, 1)
    //                        }
    //                },
    //            };

    //            VerifyCSharpDiagnostic(test, expected);
    //        }

    //        [Test]
    //        public void TestDiagnostic_WithSPDXLicenseIdentifier_WithValue_BeforeUsings_NoNamespace()
    //        {
    //            var test = @"
    //// SPDX-License-Identifier: Apache-2.0

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //public class MyClass
    //{
    //    public void MyMethod()
    //    {
    //    }
    //    public void MyMethod(int n)
    //    {
    //    }
    //    protected internal override bool LessThan(float termA, float termB)
    //    {
    //        return termA < termB;
    //    }
    //}

    //";

    //            VerifyCSharpDiagnostic(test, new[] {
    //                Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //            } );
    //        }

    //        [Test]
    //        public void TestDiagnostic_WithSPDXLicenseIdentifier_WithValue_BeforeUsings_BlockScopedNamespace()
    //        {
    //            var test = @"
    //// SPDX-License-Identifier: Apache-2.0

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //namespace MyNamespace
    //{
    //    public class MyClass
    //    {
    //        public void MyMethod()
    //        {
    //        }
    //        public void MyMethod(int n)
    //        {
    //        }
    //        protected internal override bool LessThan(float termA, float termB)
    //        {
    //            return termA < termB;
    //        }
    //    }
    //}
    //";
    //            VerifyCSharpDiagnostic(test, new[] {
    //                Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //            });
    //        }

    //        [Test]
    //        public void TestDiagnostic_WithSPDXLicenseIdentifier_WithValue_BeforeUsings_FileScopedNamespace()
    //        {
    //            var test = @"
    //// SPDX-License-Identifier: Apache-2.0

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //namespace MyNamespace;

    //public class MyClass
    //{
    //    public void MyMethod()
    //    {
    //    }
    //    public void MyMethod(int n)
    //    {
    //    }
    //    protected internal override bool LessThan(float termA, float termB)
    //    {
    //        return termA < termB;
    //    }
    //}

    //";

    //            VerifyCSharpDiagnostic(test, new[] {
    //                Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //            });
    //        }

    //        [Test]
    //        public void TestDiagnostic_WithSPDXLicenseIdentifier_WithValue_BeforeType_NoNamespace()
    //        {
    //            var test = @"

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //// SPDX-License-Identifier: Apache-2.0

    //public class MyClass
    //{
    //    public void MyMethod()
    //    {
    //    }
    //    public void MyMethod(int n)
    //    {
    //    }
    //    protected internal override bool LessThan(float termA, float termB)
    //    {
    //        return termA < termB;
    //    }
    //}
    //";

    //            VerifyCSharpDiagnostic(test, new[] {
    //                        Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                        Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    });
    //        }

    //        [Test]
    //        public void TestDiagnostic_WithSPDXLicenseIdentifier_WithValue_BeforeBlockScopedNamespace()
    //        {
    //            var test = @"

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //// SPDX-License-Identifier: Apache-2.0

    //namespace MyNamespace
    //{
    //    public class MyClass
    //    {
    //        public void MyMethod()
    //        {
    //        }
    //        public void MyMethod(int n)
    //        {
    //        }
    //        protected internal override bool LessThan(float termA, float termB)
    //        {
    //            return termA < termB;
    //        }
    //    }
    //}
    //";

    //            VerifyCSharpDiagnostic(test, new[] {
    //                        Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                        Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    });
    //        }

    //        [Test]
    //        public void TestDiagnostic_WithSPDXLicenseIdentifier_WithValue_BeforeFileScopedNamespace()
    //        {
    //            var test = @"

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //// SPDX-License-Identifier: Apache-2.0

    //namespace MyNamespace;

    //public class MyClass
    //{
    //    public void MyMethod()
    //    {
    //    }
    //    public void MyMethod(int n)
    //    {
    //    }
    //    protected internal override bool LessThan(float termA, float termB)
    //    {
    //        return termA < termB;
    //    }
    //}
    //";

    //            VerifyCSharpDiagnostic(test, new[] {
    //                        Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                        Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    });
    //        }

    //        [Test]
    //        public void TestDiagnostic_WithSPDXLicenseIdentifier_WithValue_AfterFileScopedNamespace()
    //        {
    //            var test = @"

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //namespace MyNamespace;

    //// SPDX-License-Identifier: Apache-2.0

    //public class MyClass
    //{
    //    public void MyMethod()
    //    {
    //    }
    //    public void MyMethod(int n)
    //    {
    //    }
    //    protected internal override bool LessThan(float termA, float termB)
    //    {
    //        return termA < termB;
    //    }
    //}
    //";

    //            VerifyCSharpDiagnostic(test, new[] {
    //                        Descriptors.SPDX_1000_LicenseIdentifierMustExist.Id,
    //                        Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue.Id
    //                    });
    //        }


    //        [Test]
    //        public void TestDiagnostic_WithSPDXFileCopyrightText_WithValue_BeforeUsings()
    //        {
    //            var test = @"
    //// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //public class MyClass
    //{
    //    public void MyMethod()
    //    {
    //    }
    //    public void MyMethod(int n)
    //    {
    //    }
    //    protected internal override bool LessThan(float termA, float termB)
    //    {
    //        return termA < termB;
    //    }
    //}
    //";

    //            VerifyCSharpDiagnostic(test, new[] {
    //                Descriptors.SPDX_1002_LicenseCopyrightTextMustExist.Id,
    //                Descriptors.SPDX_1003_LicenseCopyrightTextMustHaveValue.Id
    //            });
    //        }

    //        [Test]
    //        public void TestDiagnostic_WithSPDXFileCopyrightText_WithValue_AfterUsings()
    //        {
    //            var test = @"

    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith

    //public class MyClass
    //{
    //    public void MyMethod()
    //    {
    //    }
    //    public void MyMethod(int n)
    //    {
    //    }
    //    protected internal override bool LessThan(float termA, float termB)
    //    {
    //        return termA < termB;
    //    }
    //}
    //";

    //            VerifyCSharpDiagnostic(test, new[] {
    //                Descriptors.SPDX_1002_LicenseCopyrightTextMustExist.Id,
    //                Descriptors.SPDX_1003_LicenseCopyrightTextMustHaveValue.Id
    //            });
    //        }
    //    }
}
