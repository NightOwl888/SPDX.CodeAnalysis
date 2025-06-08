//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Diagnostics;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SPDX.CodeAnalysis.Tests
//{
//    public class TestSPDX1000_LicenseIdentifierExistsCSCodeAnalyzer : DiagnosticVerifier
//    {
//        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
//        {
//           return new SPDX1000_LicenseIdentifierExistsCSCodeAnalyzer();
//        }

//        [Test]
//        public void TestEmptyFile()
//        {
//            var test = @"";

//            var expected = new DiagnosticResult
//            {
//                Id = SPDX1000_LicenseIdentifierExistsCSCodeAnalyzer.DiagnosticId,
//                Message = string.Format("Header comment including '{0}' is required", "SPDX-Licence-Identifier:"),
//                Severity = DiagnosticSeverity.Warning,
//                Locations =
//                    new[] {
//                        new DiagnosticResultLocation("Test0.cs", 1, 1)
//                    }
//            };

//            VerifyCSharpDiagnostic(test, expected);
//        }

//        [Test]
//        public void TestDiagnostic_WithoutSPDXLicenseIdentifier()
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

//            var expected = new DiagnosticResult
//            {
//                Id = SPDX1000_LicenseIdentifierExistsCSCodeAnalyzer.DiagnosticId,
//                Message = string.Format("Header comment including '{0}' is required", "SPDX-Licence-Identifier:"),
//                Severity = DiagnosticSeverity.Warning,
//                Locations =
//                    new[] {
//                        new DiagnosticResultLocation("Test0.cs", 1, 1)
//                    }
//            };

//            VerifyCSharpDiagnostic(test, expected);
//        }

//            [Test]
//        public void TestDiagnostic_WithSPDXLicenseIdentifier_BeforeUsings()
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

//            VerifyCSharpDiagnostic(test);
//        }

//        [Test]
//        public void TestDiagnostic_WithSPDXLicenseIdentifier_AfterUsings()
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

//            VerifyCSharpDiagnostic(test);
//        }
//    }
//}
