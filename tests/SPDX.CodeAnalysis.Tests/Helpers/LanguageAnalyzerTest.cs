// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.VisualBasic;
using System;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis.Tests
{
    /// <summary>
    /// Test driver for the provided <see cref="CodeLanguage"/>. This class facilitates the use
    /// of dependency injection and allows subclasses to be extended and reused across languages.
    /// </summary>
    /// <typeparam name="TVerifier">The type of verifier to use.</typeparam>
    public abstract class LanguageAnalyzerTest<TVerifier> : AnalyzerTest<TVerifier>
        where TVerifier : IVerifier, new()
    {
        private readonly CodeLanguage language;

        protected LanguageAnalyzerTest(CodeLanguage language)
        {
            this.language = language;
        }

        public override string Language => language switch
        {
            CodeLanguage.CSharp => LanguageNames.CSharp,
            CodeLanguage.VisualBasic => LanguageNames.VisualBasic,
            _ => throw new NotSupportedException()
        };

        protected override string DefaultFileExt => language switch
        {
            CodeLanguage.CSharp => "cs",
            CodeLanguage.VisualBasic => "vb",
            _ => throw new NotSupportedException()
        };

        protected override ParseOptions CreateParseOptions() => language switch
        {
            CodeLanguage.CSharp => new CSharpParseOptions(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest, DocumentationMode.Diagnose),
            CodeLanguage.VisualBasic => new VisualBasicParseOptions(documentationMode: DocumentationMode.Diagnose),
            _ => throw new NotSupportedException()
        };

        protected override CompilationOptions CreateCompilationOptions() => language switch
        {
            CodeLanguage.CSharp => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            CodeLanguage.VisualBasic => new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            _ => throw new NotSupportedException()
        };

        protected abstract DiagnosticAnalyzer CreateCSharpAnalyzer();
        protected abstract DiagnosticAnalyzer CreateVisualBasicAnalyzer();

        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers() => language switch
        {
            CodeLanguage.CSharp => new[] { CreateCSharpAnalyzer() },
            CodeLanguage.VisualBasic => new[] { CreateVisualBasicAnalyzer() },
            _ => throw new NotSupportedException()
        };
    }
}
