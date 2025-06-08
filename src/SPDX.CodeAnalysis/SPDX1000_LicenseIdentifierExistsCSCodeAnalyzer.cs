//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Diagnostics;
//using Microsoft.CodeAnalysis.Text;
//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Resources;
//using System.Threading;

//namespace SPDX.CodeAnalysis
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    public class SPDX1000_LicenseIdentifierExistsCSCodeAnalyzer : DiagnosticAnalyzer
//    {
//        public const string DiagnosticId = "SPDX_1000";

//        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
//        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
//        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.SPDX_1000_AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
//        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.SPDX_1000_AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
//        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.SPDX_1000_AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
//        private const string Category = "Licensing";

//        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

//        public override void Initialize(AnalysisContext context)
//        {
//            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
//            context.EnableConcurrentExecution();
//            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
//        }

//        private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
//        {
//            var root = context.Tree.GetRoot(context.CancellationToken);
//            var firstToken = root.GetFirstToken(includeZeroWidth: true);

//            var triviaList = firstToken.LeadingTrivia;

//            bool hasLicenseId = false;

//            foreach (var trivia in triviaList)
//            {
//                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
//                {
//                    var commentText = trivia.ToString().TrimStart('/', ' ').Trim();

//                    if (commentText.StartsWith("SPDX-License-Identifier:"))
//                        hasLicenseId = true;

//                    //if (commentText.StartsWith("SPDX-FileCopyrightText:"))
//                    //    hasCopyright = true;

//                    // Stop scanning if found
//                    if (hasLicenseId)
//                        break;
//                }
//                else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
//                {
//                    var lines = trivia.ToString().Split('\n');
//                    foreach (var line in lines)
//                    {
//                        var trimmed = line.Trim('/', '*', ' ');
//                        if (trimmed.StartsWith("SPDX-License-Identifier:"))
//                            hasLicenseId = true;
//                        //if (trimmed.StartsWith("SPDX-FileCopyrightText:"))
//                        //    hasCopyright = true;
//                    }
//                }
//            }

//            if (!hasLicenseId)
//            {
//                context.ReportDiagnostic(Diagnostic.Create(Rule, Location.Create(context.Tree, new TextSpan(0, 0))));
//            }
//        }
//    }
//}
