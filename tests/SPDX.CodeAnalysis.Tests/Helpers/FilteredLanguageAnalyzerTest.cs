// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Model;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    public abstract class FilteredLanguageAnalyzerTest<TVerifier> : LanguageAnalyzerTest<TVerifier>
        where TVerifier : IVerifier, new()
    {
        public FilteredLanguageAnalyzerTest(CodeLanguage language) : base(language)
        {
        }

        /// <summary>
        /// Gets a collection of diagnostics to explicitly enable in the <see cref="CompilationOptions"/> for projects.
        /// Any diagnostics in <see cref="EmptyDiagnosticAnalyzer"/>
        /// </summary>
        public List<string> EnabledDiagnostics { get; } = new List<string>();

        /// <inheritdoc/>
        protected override async Task RunImplAsync(CancellationToken cancellationToken)
        {
            if (!TestState.GeneratedSources.Any())
            {
                // Verify the test state has at least one source, which may or may not be generated
                Verify.NotEmpty($"{nameof(TestState)}.{nameof(SolutionState.Sources)}", TestState.Sources);
            }

            var analyzers = GetDiagnosticAnalyzers().ToArray();
            var defaultDiagnostic = GetDefaultDiagnostic(analyzers);
            var supportedDiagnostics = analyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics).ToImmutableArray();

            // If filtered, disable the diagnostics for any supported ids
            var enabledDiagnostics = EnabledDiagnostics;
            if (enabledDiagnostics is not null && enabledDiagnostics.Count > 0)
            {
                var disabledDiagnostics = DisabledDiagnostics;
                foreach (var diag in supportedDiagnostics)
                {
                    if (!enabledDiagnostics.Contains(diag.Id) && !disabledDiagnostics.Contains(diag.Id))
                    {
                        disabledDiagnostics.Add(diag.Id);
                    }
                }
            }

            var fixableDiagnostics = ImmutableArray<string>.Empty;
            var testState = TestState.WithInheritedValuesApplied(null, fixableDiagnostics).WithProcessedMarkup(MarkupOptions, defaultDiagnostic, supportedDiagnostics, fixableDiagnostics, DefaultFilePath);

            var diagnostics = await VerifySourceGeneratorAsync(testState, Verify, cancellationToken).ConfigureAwait(false);
            await VerifyDiagnosticsAsync(new EvaluatedProjectState(testState, ReferenceAssemblies), testState.AdditionalProjects.Values.Select(additionalProject => new EvaluatedProjectState(additionalProject, ReferenceAssemblies)).ToImmutableArray(), testState.ExpectedDiagnostics.ToArray(), Verify, cancellationToken).ConfigureAwait(false);
        }
    }
}
