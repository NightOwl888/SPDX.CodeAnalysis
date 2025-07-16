using Microsoft.CodeAnalysis;
using SPDX.CodeAnalysis.CSharp;
using System.Collections.Concurrent;

namespace SPDX.CodeAnalysis
{
    public static partial class Descriptors
    {
        static readonly ConcurrentDictionary<Category, string> categoryMapping = new();

        static DiagnosticDescriptor Diagnostic(
            string id,
            Category category,
            DiagnosticSeverity defaultSeverity)
        {
            string? helpLink = null;
            var categoryString = categoryMapping.GetOrAdd(category, c => c.ToString());

            var title = new LocalizableResourceString($"{id}_AnalyzerTitle", Resources.ResourceManager, typeof(Resources));
            var messageFormat = new LocalizableResourceString($"{id}_AnalyzerMessageFormat", Resources.ResourceManager, typeof(Resources));
            var description = new LocalizableResourceString($"{id}_AnalyzerDescription", Resources.ResourceManager, typeof(Resources));

            return new DiagnosticDescriptor(id, title, messageFormat, categoryString, defaultSeverity, isEnabledByDefault: true, helpLinkUri: helpLink);
        }

        //static DiagnosticDescriptor Diagnostic(
        //    string id,
        //    string title,
        //    Category category,
        //    DiagnosticSeverity defaultSeverity,
        //    string messageFormat)
        //{
        //    string? helpLink = null;
        //    var categoryString = categoryMapping.GetOrAdd(category, c => c.ToString());

        //    return new DiagnosticDescriptor(id, title, messageFormat, categoryString, defaultSeverity, isEnabledByDefault: true, helpLinkUri: helpLink);
        //}

        static SuppressionDescriptor Suppression(
            string suppressedDiagnosticId,
            string justification) =>
                new("SPDXSuppress-" + suppressedDiagnosticId, suppressedDiagnosticId, justification);
    }
}
