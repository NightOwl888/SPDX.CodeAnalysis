using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// Base class for diagnostic analyzers which support license header validation.
    /// </summary>
    public abstract class SPDXDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private readonly DiagnosticDescriptor[] descriptors;

        public SPDXDiagnosticAnalyzer(params DiagnosticDescriptor[] descriptors)
        {
            this.descriptors = descriptors ?? throw new ArgumentNullException(nameof(descriptors));
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => descriptors.ToImmutableArray();
    }
}
