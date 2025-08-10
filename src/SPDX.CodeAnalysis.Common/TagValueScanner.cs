// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using System;

namespace SPDX.CodeAnalysis
{
    public sealed class TagValueScanner : ITagValueScanner
    {
        private readonly ILanguageService languageService;

        public TagValueScanner(ILanguageService languageService)
        {
            this.languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        }

        public bool ScanForTagAndValue(SyntaxNode root, ref TagValueSession spdxLicenseIdentifierSession, ref TagValueSession spdxFileCopyrightTextSession)
        {
            if (root is null)
                throw new ArgumentNullException(nameof(root));

            foreach (var token in root.DescendantTokens())
            {
                // Process leading trivia for each token
                if (ProcessSpdxTag(token.LeadingTrivia, ref spdxLicenseIdentifierSession, ref spdxFileCopyrightTextSession))
                    return true;

                // Stop scanning once we hit the first type declaration
                if (languageService.IsTypeDeclarationSyntax(token.Parent))
                    return false;
            }

            return false;
        }

        private bool ProcessSpdxTag(SyntaxTriviaList triviaList,
            ref TagValueSession spdxLicenseIdentifierSession,
            ref TagValueSession spdxFileCopyrightTextSession)
        {
            bool hasLicenseIdentifier = false, hasFileCopyrightText = false;

            foreach (var trivia in triviaList)
            {
                if (languageService.IsKind(trivia, LanguageAgnosticSyntaxKind.SingleLineCommentTrivia))
                {
                    string text = trivia.ToString(); // safe to reference span from here
                    ReadOnlySpan<char> line = text.AsSpan();

                    hasLicenseIdentifier |= spdxLicenseIdentifierSession.TryFindTag(text, line);
                    hasFileCopyrightText |= spdxFileCopyrightTextSession.TryFindTag(text, line);
                }
                else if (languageService.IsKind(trivia, LanguageAgnosticSyntaxKind.MultiLineCommentTrivia))
                {
                    string text = trivia.ToString(); // safe to reference span from here

                    int lineOffset = 0;
                    foreach (var line in text.SplitLines())
                    {
                        hasLicenseIdentifier |= spdxLicenseIdentifierSession.TryFindTag(text, line, lineOffset);
                        hasFileCopyrightText |= spdxFileCopyrightTextSession.TryFindTag(text, line, lineOffset);

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
    }
}
