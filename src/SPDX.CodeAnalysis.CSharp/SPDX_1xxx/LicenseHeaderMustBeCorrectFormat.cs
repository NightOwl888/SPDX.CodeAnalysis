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

namespace SPDX.CodeAnalysis.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LicenseHeaderMustBeCorrectFormat : DiagnosticAnalyzer
    {
        private const string LicenseIdentifierTag = "SPDX-License-Identifier";
        private const string FileCopyrightTextTag = "SPDX-FileCopyrightText";
        private const string TopLevelDirectoryName = "LICENSES.HEADERS"; // TODO: Make configurable?
        private const int SingleLineCommentTriviaKind = (int)SyntaxKind.SingleLineCommentTrivia;
        private const int MultiLineCommentTriviaKind = (int)SyntaxKind.MultiLineCommentTrivia;
        private const int WhitespaceTriviaKind = (int)SyntaxKind.WhitespaceTrivia;

        private static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray.Create<DiagnosticDescriptor>(
            Descriptors.SPDX_1000_LicenseIdentifierMustExist,
            Descriptors.SPDX_1001_LicenseIdentifierMustHaveValue,
            Descriptors.SPDX_1002_FileCopyrightTextMustExist,
            Descriptors.SPDX_1003_FileCopyrightTextMustHaveValue,
            Descriptors.SPDX_1004_LicenseCopyrightTextMustPrecedeLicenseIdentifier,
            Descriptors.SPDX_1005_LicenseTextMustExist,
            Descriptors.SPDX_2000_NoLicenseHeaderTextConfiguration
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
                bool hasMatchingLicenseText = false;

                ReadOnlySpan<char> spdxLicenseIdentifier = spdxLicenseIdentifierSession.Value.TrimEnd();

                // First pass: Optimize the search for the "happy path" where the SPDX-License-Identifier and codeFilePath match the license header text(s)
                IReadOnlyList<LicenseHeaderCacheText> matchingLicenseHeaderTexts = cache.GetMatchingLicenseHeaders(spdxLicenseIdentifier, codeFilePath);

                if (matchingLicenseHeaderTexts.Count > 0)
                {
                    //// Fast path: In the typical case there will be only one license header text to match against.
                    //if (matchingLicenseHeaderTexts.Count == 1)
                    //{
                    //    //IReadOnlyList<string> lines = matchingLicenseHeaderTexts[0].Lines;
                    //    LicenseHeaderMatchSession session = new LicenseHeaderMatchSession(matchingLicenseHeaderTexts[0]);

                    //    foreach (var token in root.DescendantTokens())
                    //    {
                    //        //if (ProcessLicenseText(lines, token.LeadingTrivia))
                    //        //    break;

                    //        //if (session.TryMatchNextLine())

                    //        // Stop scanning once we hit the first type declaration
                    //        if (token.Parent is TypeDeclarationSyntax)
                    //            break;
                    //    }
                    //}
                    //else
                    //{
                    //    // TODO: We need our session to track matches for each matchingLicenseHeaderTexts
                    //    // and determine if any of them matched (and set hasLicnseText to true). If not, we proceed to the second pass.


                    //}
                    int sessionCount = matchingLicenseHeaderTexts.Count;
                    LicenseHeaderMatchSession[] matchSessions = new LicenseHeaderMatchSession[sessionCount];
                    for (int i = 0; i < sessionCount; i++)
                    {
                        matchSessions[i] = new LicenseHeaderMatchSession(matchingLicenseHeaderTexts[i], WhitespaceTriviaKind, SingleLineCommentTriviaKind, MultiLineCommentTriviaKind);
                    }

                    foreach (var token in root.DescendantTokens())
                    {
                        if (!ProcessLicenseText(matchSessions, token.LeadingTrivia))
                            break;

                        // Stop scanning once we hit the first type declaration
                        if (token.Parent is TypeDeclarationSyntax)
                            break;
                    }

                    for (int i = 0; i < sessionCount; i++)
                    {
                        hasMatchingLicenseText |= matchSessions[i].IsFullMatch;
                    }

                }

                if (!hasMatchingLicenseText)
                {
                    // Second pass: We know we need to report a diagnostic now, but we are being more thorough with the matching.
                    // We run a second pass while recording position information so we can report the diagnostics.
                    IReadOnlyList<LicenseHeaderCacheText> allLicenseHeaderTexts = cache.GetAllLicenseHeaders();

                    int sessionCount = allLicenseHeaderTexts.Count;
                    LicenseHeaderMatchSession[] matchSessions = new LicenseHeaderMatchSession[sessionCount];
                    for (int i = 0; i < sessionCount; i++)
                    {
                        matchSessions[i] = new LicenseHeaderMatchSession(allLicenseHeaderTexts[i], WhitespaceTriviaKind, SingleLineCommentTriviaKind, MultiLineCommentTriviaKind);
                    }

                    foreach (var token in root.DescendantTokens())
                    {
                        if (!ProcessLicenseText(matchSessions, token.LeadingTrivia))
                            break;

                        // Stop scanning once we hit the first type declaration
                        if (token.Parent is TypeDeclarationSyntax)
                            break;
                    }

                    bool hasAnyFullMatch = false;
                    bool hasAnyPartialMatch = false;
                    for (int i = 0; i < sessionCount; i++)
                    {
                        hasAnyFullMatch |= matchSessions[i].IsFullMatch;
                        hasAnyPartialMatch |= matchSessions[i].IsPartialMatch;
                    }

                    // TODO: We need our session to track matches for each allLicenseHeaderTexts
                    // and determine if any of them matched. This time, we need to enable position tracking
                    // so we can report a diagnostic.

                    ReportHasNoLicenseText(context);
                }
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
        }

        private static bool ProcessLicenseText(LicenseHeaderMatchSession[] matchSessions, SyntaxTriviaList triviaList)
        {
            foreach (var trivia in triviaList)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    string text = trivia.ToString(); // safe to reference span from here
                    ReadOnlySpan<char> line = text.AsSpan();
                    int absoluteOffset = trivia.Span.Start;
                    if (!ProcessLicenseTextLine(matchSessions, trivia, line, absoluteOffset))
                        return false;
                }
                else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    string text = trivia.ToString(); // safe to reference span from here

                    int lineOffset = 0;
                    foreach (var line in text.SplitLines())
                    {
                        int absoluteOffset = trivia.Span.Start + lineOffset;

                        if (!ProcessLicenseTextLine(matchSessions, trivia, line, absoluteOffset))
                            return false;

                        lineOffset += line.Line.Length + line.Separator.Length;
                    }
                }
                else
                {
                    if (!ProcessLicenseNonCommentTrivia(matchSessions, trivia))
                        return false;
                }
            }

            return true; // Continue processing
        }

        private static bool ProcessLicenseTextLine(LicenseHeaderMatchSession[] matchSessions, SyntaxTrivia trivia, ReadOnlySpan<char> line, int absoluteOffset)
        {
            bool anyActive = false;
            for (int i = 0; i < matchSessions.Length; i++)
            {
                anyActive |= matchSessions[i].MatchNextLine(trivia, line, absoluteOffset);
            }
            return anyActive;
        }

        private static bool ProcessLicenseNonCommentTrivia(LicenseHeaderMatchSession[] matchSessions, SyntaxTrivia trivia)
        {
            bool anyActive = false;
            for (int i = 0; i < matchSessions.Length; i++)
            {
                anyActive |= matchSessions[i].NotifyNonCommentTrivia(trivia);
            }
            return anyActive;
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
                    string text = trivia.ToString(); // safe to reference span from here
                    ReadOnlySpan<char> line = text.AsSpan();

                    hasLicenseIdentifier = spdxLicenseIdentifierSession.TryFindTag(text, line);
                    hasFileCopyrightText = spdxFileCopyrightTextSession.TryFindTag(text, line);
                }
                else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    string text = trivia.ToString(); // safe to reference span from here

                    int lineOffset = 0;
                    foreach (var line in text.SplitLines())
                    {
                        hasLicenseIdentifier = spdxLicenseIdentifierSession.TryFindTag(text, line, lineOffset);
                        hasFileCopyrightText = spdxFileCopyrightTextSession.TryFindTag(text, line, lineOffset);

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
            => context.ReportDiagnostic(Diagnostic.Create(Descriptors.SPDX_2000_NoLicenseHeaderTextConfiguration, Location.None));
    }
}
