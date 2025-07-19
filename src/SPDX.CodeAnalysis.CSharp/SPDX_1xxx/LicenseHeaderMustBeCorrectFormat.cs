// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace SPDX.CodeAnalysis.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LicenseHeaderMustBeCorrectFormat : DiagnosticAnalyzer
    {
        private const string LicenseIdentifierTag = "SPDX-License-Identifier";
        private const string FileCopyrightTextTag = "SPDX-FileCopyrightText";
        private const string TopLevelDirectoryName = "LICENSES.HEADERS"; // TODO: Make configurable?
        private static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray.Create<DiagnosticDescriptor>(
            Descriptors.SPDX_1000_LicenseIdentifierMustExist,
            Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue,
            Descriptors.SPDX_1002_FileCopyrightTextMustExist,
            Descriptors.SPDX_1003_FileCopyrightTextMustHaveValue,
            Descriptors.SPDX_1004_LicenseCopyrightTextMustPrecedeLicenseIdentifier,
            Descriptors.SPDX_1005_LicenseTextMustExist,
            Descriptors.SPDX_1006_NoLicenseHeaderTextConfiguration
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => supportedDiagnostics;

        // Dependency injection
        private readonly LicenseAnalyzerOptions options;

        public LicenseHeaderMustBeCorrectFormat()
            : this(options: null)
        {
        }

        // Dependency injection constructor (for testing)
        internal LicenseHeaderMustBeCorrectFormat(LicenseAnalyzerOptions? options)
        {
            this.options = options ?? new LicenseAnalyzerOptions();
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            // Load the configuration from AdditionalFiles
            var loader = new AdditionalFilesLicenseHeaderCacheLoader(context.Options.AdditionalFiles);
            // Create a per-compilation cache instance.
            // This lifetime ensures the cache is reloaded if any of the AdditionalFiles changes.
            var cache = new LicenseHeaderCache();
            
            // codeFilePath is not used by this loader
            cache.EnsureInitialized(loader, codeFilePath: string.Empty, TopLevelDirectoryName);

            // Register actions that analyze syntax trees
            context.RegisterSyntaxTreeAction((ctx) => AnalyzeSyntaxTree(ctx, cache));

            // Report missing config if applicable
            context.RegisterCompilationEndAction((ctx) => OnCompilationEnd(ctx, cache));
        }

        private void OnCompilationEnd(CompilationAnalysisContext context, LicenseHeaderCache cache)
        {
            if (cache.IsEmpty)
            {
                ReportHasNoLicenseHeaderTextConfiguration(context);
            }
        }

        private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context, LicenseHeaderCache cache)
        {
            var root = context.Tree.GetRoot(context.CancellationToken);
            //var firstToken = root.GetFirstToken(includeZeroWidth: true);

            //var triviaList = firstToken.LeadingTrivia;
            string codeFilePath = context.Tree.FilePath;

            bool hasLicenseText = false;
            bool hasLicenseTextPartial = false;

            var spdxLicenseIdentifierSession = new TagValueSession(LicenseIdentifierTag);
            var spdxFileCopyrightTextSession = new TagValueSession(FileCopyrightTextTag);

            foreach (var token in root.DescendantTokens())
            {
                // Process leading trivia for each token
                if (ProcessSpdxTag(token.LeadingTrivia, ref spdxLicenseIdentifierSession, ref spdxFileCopyrightTextSession))
                    break;

                // Stop scanning once we hit the first type declaration
                if (token.Parent is TypeDeclarationSyntax)
                    break;
            }

            // Second pass - If we found the SPDX tags, then look for the correct license header text.
            // This is configured as a file in a LICENSES.HEADERS directory which may be levels above
            // the current directory.
            if (spdxLicenseIdentifierSession.HasTag && spdxLicenseIdentifierSession.HasValue)
            {
                ReadOnlySpan<char> spdxLicenseIdentifier = spdxLicenseIdentifierSession.Value.TrimEnd();

                // First pass: Optimize the search for the "happy path" where the SPDX-License-Identifier and codeFilePath match the license header text(s)
                IReadOnlyList<LicenseHeaderCacheText> matchingLicenseHeaderTexts = cache.GetMatchingLicenseHeaders(spdxLicenseIdentifier, codeFilePath);

                if (matchingLicenseHeaderTexts.Count > 0)
                {
                    // Fast path: In the typical case there will be only one license header text to match against.
                    if (matchingLicenseHeaderTexts.Count == 1)
                    {
                        IReadOnlyList<string> lines = matchingLicenseHeaderTexts[0].Lines;

                        foreach (var token in root.DescendantTokens())
                        {
                            if (ProcessLicenseText(lines, token.LeadingTrivia))
                                break;

                            // Stop scanning once we hit the first type declaration
                            if (token.Parent is TypeDeclarationSyntax)
                                break;
                        }
                    }
                    else
                    {
                        // TODO: We need our session to track matches for each matchingLicenseHeaderTexts
                        // and determine if any of them matched (and set hasLicnseText to true). If not, we proceed to the second pass.
                    }
                }

                if (!hasLicenseText)
                {
                    // Second pass: We know we need to report a diagnostic now, but we are being more thorough with the matching.
                    // We run a second pass while recording position information so we can report the diagnostics.
                    IReadOnlyList<LicenseHeaderCacheText> allLicenseHeaderTexts = cache.GetAllLicenseHeaders();

                    if (allLicenseHeaderTexts.Count == 0)
                    {
                        // TODO: Report diagnostic because there are no configured license header texts.
                    }

                    // TODO: We need our session to track matches for each allLicenseHeaderTexts
                    // and determine if any of them matched. This time, we need to enable position tracking
                    // so we can report a diagnostic.
                }


                // TODO: Re-implement this so we have reporting on license header matching
                //if (!provider.TryGetLicenseHeader(directory, spdxLicenseIdentifier, out IReadOnlyList<string> licenseTextLines))
                //{
                //    // TODO: Report this as an unconfigured license header. For now, we have no fallback values.
                //}

                //foreach (var token in root.DescendantTokens())
                //{
                //    if (ProcessLicenseText(licenseTextLines, token.LeadingTrivia))
                //        break;

                //    // Stop scanning once we hit the first type declaration
                //    if (token.Parent is TypeDeclarationSyntax)
                //        break;
                //}
            }

            //bool ProcessLicenseTextLine(IReadOnlyList<string> licenseTextLines, ref int licenseTextLinesIndex, SyntaxTrivia trivia, string text, ReadOnlySpan<char> line, int separatorOffset = 0)
            //{
            //    ReadOnlySpan<char> licenseLine = licenseTextLines[licenseTextLinesIndex].AsSpan();

            //    int offset = line.IndexOf(licenseLine, StringComparison.Ordinal);
            //    if (offset > -1)
            //    {
            //        licenseTextLinesIndex++;
            //        if (licenseTextLinesIndex == licenseTextLines.Count)
            //            return true;
            //    }

            //    return false;
            //}

            bool ProcessLicenseText(IReadOnlyList<string> licenseTextLines, SyntaxTriviaList triviaList)
            {
                var matchSession = new LicenseHeaderMatchSession(licenseTextLines);

                // Keep track of the line to match in licenseTextLines
                //int licenseTextLinesIndex = 0;

                foreach (var trivia in triviaList)
                {
                    if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                    {
                        string text = trivia.ToString(); // safe to reference span from here
                        if (matchSession.MatchNextLine(text.AsSpan()))
                            hasLicenseText = true;

                        //ProcessLicenseTextLine(licenseTextLines, ref licenseTextLinesIndex, trivia, text, text.AsSpan());
                    }
                    else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                    {
                        string text = trivia.ToString(); // safe to reference span from here

                        //int separatorOffset = 0;
                        foreach (var line in text.SplitLines())
                        {
                            //ProcessLicenseTextLine(licenseTextLines, ref licenseTextLinesIndex, trivia, text, line, separatorOffset);
                            if (matchSession.MatchNextLine(line))
                            {
                                hasLicenseText = true;
                                break;
                            }


                            //// Stop scanning if found
                            //if (hasLicenseText)
                            //    return true;

                            //separatorOffset += line.Separator.Length;
                        }
                    }

                    // Stop scanning if found
                    if (hasLicenseText)
                        break;
                }

                if (matchSession.IsComplete)
                {
                    hasLicenseTextPartial = false;
                }
                else if (matchSession.HasMatchedAny)
                {
                    hasLicenseTextPartial = true;
                }
                return false;
            }

            if (!spdxLicenseIdentifierSession.HasTag)
            {
                ReportHasNoLicenseIdentifier(context);
            }
            else if (!spdxLicenseIdentifierSession.HasValue)
            {
                ReportHasNoLicenseIdentifierValue(context,
                    new TextSpan(spdxLicenseIdentifierSession.ValueOffsetInText, spdxLicenseIdentifierSession.ValueLength));
            }

            if (!spdxFileCopyrightTextSession.HasTag)
            {
                ReportHasNoFileCopyrightText(context);
            }
            else if (!spdxFileCopyrightTextSession.HasValue)
            {
                ReportHasNoFileCopyrightTextValue(context,
                    new TextSpan(spdxFileCopyrightTextSession.ValueOffsetInText, spdxFileCopyrightTextSession.ValueLength));
            }

            if (!hasLicenseText)
            {
                ReportHasNoLicenseText(context);
            }
        }

        private static bool ProcessSpdxTag(SyntaxTriviaList triviaList,
            ref TagValueSession spdxLicenseIdentifierSession,
            ref TagValueSession spdxFileCopyrightTextSession)
        {
            bool hasLicenseIdentifier = false, hasFileCopyrightText = false;
         
            foreach (var trivia in triviaList)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    string textString = trivia.ToString(); // safe to reference span from here
                    ReadOnlySpan<char> text = textString.AsSpan();

                    hasLicenseIdentifier = spdxLicenseIdentifierSession.TryFindTag(trivia, textString, text);
                    hasFileCopyrightText = spdxFileCopyrightTextSession.TryFindTag(trivia, textString, text);
                }
                else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    string textString = trivia.ToString(); // safe to reference span from here

                    int lineOffset = 0;
                    foreach (var line in textString.SplitLines())
                    {
                        hasLicenseIdentifier = spdxLicenseIdentifierSession.TryFindTag(trivia, textString, line, lineOffset);
                        hasFileCopyrightText = spdxFileCopyrightTextSession.TryFindTag(trivia, textString, line, lineOffset);

                        // Stop scanning if found
                        if (hasLicenseIdentifier && hasFileCopyrightText)
                            return true;

                        lineOffset += line.Line.Length + line.Separator.Length;
                    }
                }

                // Stop scanning if found
                if (hasLicenseIdentifier && hasFileCopyrightText)
                    return true;
            }
            return false;
        }

        private void ReportDiagnostic(SyntaxTreeAnalysisContext context, DiagnosticDescriptor descriptor, TextSpan span)
        {
            var location = options.SuppressLocation
                ? Location.None
                : Location.Create(context.Tree, span);

            var diagnostic = Diagnostic.Create(descriptor, location);
            context.ReportDiagnostic(diagnostic);
        }

        private void ReportHasNoLicenseIdentifier(SyntaxTreeAnalysisContext context)
            => ReportDiagnostic(context, Descriptors.SPDX_1000_LicenseIdentifierMustExist, new TextSpan(0, 0));

        private void ReportHasNoLicenseIdentifierValue(SyntaxTreeAnalysisContext context, TextSpan span)
            => ReportDiagnostic(context, Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue, span);

        private void ReportHasNoFileCopyrightText(SyntaxTreeAnalysisContext context)
            => ReportDiagnostic(context, Descriptors.SPDX_1002_FileCopyrightTextMustExist, new TextSpan(0, 0));

        private void ReportHasNoFileCopyrightTextValue(SyntaxTreeAnalysisContext context, TextSpan span)
            => ReportDiagnostic(context, Descriptors.SPDX_1003_FileCopyrightTextMustHaveValue, span);

        private void ReportHasNoLicenseText(SyntaxTreeAnalysisContext context)
            => ReportDiagnostic(context, Descriptors.SPDX_1005_LicenseTextMustExist, new TextSpan(0, 0));

        private void ReportHasNoLicenseHeaderTextConfiguration(CompilationAnalysisContext context)
            => context.ReportDiagnostic(Diagnostic.Create(Descriptors.SPDX_1006_NoLicenseHeaderTextConfiguration, Location.None));
    }
}
