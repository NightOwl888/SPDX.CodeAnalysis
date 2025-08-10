// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis
{
    public sealed class LicenseHeaderScanner : ILicenseHeaderScanner
    {
        private readonly ILanguageService languageService;

        public LicenseHeaderScanner(ILanguageService languageService)
        {
            this.languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        }

        public bool TryScanForLicenseHeaders(SyntaxNode root, IReadOnlyList<LicenseHeaderCacheText> configuredLicenseHeaders, out IReadOnlyList<LicenseHeaderMatchSession> result)
        {
            if (root is null)
                throw new ArgumentNullException(nameof(root));

            int sessionCount = configuredLicenseHeaders.Count;
            LicenseHeaderMatchSession[] matchSessions = new LicenseHeaderMatchSession[sessionCount];
            for (int i = 0; i < sessionCount; i++)
            {
                matchSessions[i] = new LicenseHeaderMatchSession(configuredLicenseHeaders[i], languageService);
            }
            result = matchSessions;

            foreach (var token in root.DescendantTokens())
            {
                if (!ProcessLicenseText(matchSessions, token.LeadingTrivia))
                    return true;

                // Stop scanning once we hit the first type declaration
                if (languageService.IsTypeDeclarationSyntax(token.Parent))
                    return false;
            }

            return false;
        }

        private bool ProcessLicenseText(LicenseHeaderMatchSession[] matchSessions, SyntaxTriviaList triviaList)
        {
            foreach (var trivia in triviaList)
            {
                if (languageService.IsKind(trivia, LanguageAgnosticSyntaxKind.SingleLineCommentTrivia))
                {
                    string text = trivia.ToString(); // safe to reference span from here
                    ReadOnlySpan<char> line = text.AsSpan();
                    int absoluteOffset = trivia.Span.Start;
                    if (!ProcessLicenseTextLine(matchSessions, trivia, line, absoluteOffset))
                        return false;
                }
                else if (languageService.IsKind(trivia, LanguageAgnosticSyntaxKind.MultiLineCommentTrivia))
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
    }
}
