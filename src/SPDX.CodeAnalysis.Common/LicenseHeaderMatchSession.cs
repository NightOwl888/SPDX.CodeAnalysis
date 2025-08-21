// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SPDX.CodeAnalysis.Languages;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// Performs line-by-line matching of an expected license header against a block of trivia lines.
    /// Tracks whether the lines matched exactly (in order), partially, or mismatched.
    /// Does not evaluate SPDX or directory path matches — only textual license content.
    /// </summary>
    public struct LicenseHeaderMatchSession
    {
        private /*readonly*/ ILanguageService languageService;

        private /*readonly*/ string matchDirectoryPath;
        private /*readonly*/ IReadOnlyList<string> expectedLines;

        private SyntaxTrivia? lastMatchedTrivia;
        private SyntaxTrivia? lastTriviaChecked;
        private SyntaxTrivia? firstMismatchedTrivia;

        private int matchedLineCount;
        private int lastMatchedLineOffset;
        private int startOffset;
        private int endOffset;
        private int? mismatchStartOffset;
        private bool hasStarted;
        private bool hasCompleted;
        private bool shouldStopMatching;
        private bool isContiguous;

        /// <summary>
        /// Initializes the session using a license header from configuration.
        /// </summary>
        public LicenseHeaderMatchSession(LicenseHeaderCacheText header, ILanguageService languageService)
        {
            this.languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
            if (header is null)
                throw new ArgumentNullException(nameof(header));
            expectedLines = header.Lines;
            matchDirectoryPath = header.MatchDirectoryPath;

            matchedLineCount = 0;
            lastMatchedLineOffset = -1;
            startOffset = -1;
            endOffset = -1;
            hasStarted = false;
            hasCompleted = false;
            shouldStopMatching = false;
            isContiguous = true;
        }

        /// <summary>
        /// Attempts to match the next trivia line against the expected license line.
        /// Lines must match in order and be contiguous in character offset.
        /// </summary>
        /// <param name="trivia">The trivia block that contains the <paramref name="triviaLine"/>.</param>
        /// <param name="triviaLine">The line of text (e.g., from a trivia comment).</param>
        /// <param name="absoluteOffset">The character offset of the start of this line within the full trivia block.</param>
        /// <returns><c>true</c> if the session is still active (we haven't matched all lines yet); otherwise, <c>false</c>.</returns>
        public bool MatchNextLine(SyntaxTrivia trivia, ReadOnlySpan<char> triviaLine, int absoluteOffset)
        {
            if (matchedLineCount >= expectedLines.Count || shouldStopMatching)
                return false;

            var expectedLine = expectedLines[matchedLineCount];
            int index = triviaLine.IndexOf(expectedLine.AsSpan(), StringComparison.Ordinal);
            if (index == -1)
            {
                if (hasStarted)
                {
                    // First mismatch during active match
                    if (firstMismatchedTrivia is null)
                    {
                        mismatchStartOffset = absoluteOffset;
                        firstMismatchedTrivia = trivia;
                    }

                    // Mark the end even on mismatch after some progress
                    shouldStopMatching = true;

                    // Don't change isContiguous unless a real gap is encountered.
                    // Let NotifyNonCommentTrivia() control that.
                }

                lastTriviaChecked = trivia;
                return !MatchSessionEnded;
            }

            if (!hasStarted)
            {
                hasStarted = true;
                startOffset = absoluteOffset + index;
                lastMatchedTrivia = trivia;
            }
            else if (!SyntaxTriviaAreSame(lastMatchedTrivia, trivia) &&
                !IsTriviaContiguous(lastMatchedTrivia, trivia))
            {
                isContiguous = false;
            }

            lastMatchedLineOffset = absoluteOffset;
            matchedLineCount++;
            lastMatchedTrivia = trivia;
            lastTriviaChecked = trivia;

            if (matchedLineCount == expectedLines.Count)
                hasCompleted = true;

            endOffset = absoluteOffset + triviaLine.Length;
            return !MatchSessionEnded;
        }

        private bool IsTriviaContiguous(SyntaxTrivia? previous, SyntaxTrivia current)
        {
            // Must be from the same tree
            if (previous is null || previous.Value.SyntaxTree != current.SyntaxTree)
                return false;

            SyntaxTrivia prev = previous.Value;

            if (languageService.IsKind(prev, LanguageAgnosticSyntaxKind.SingleLineCommentTrivia) &&
                languageService.IsKind(current, LanguageAgnosticSyntaxKind.SingleLineCommentTrivia))
            {
                // We are always loading a SyntaxTree, so we can ignore nullability here
                var prevLine = prev.SyntaxTree!.GetLineSpan(prev.Span).EndLinePosition.Line;
                var currLine = current.SyntaxTree!.GetLineSpan(current.Span).StartLinePosition.Line;
                return currLine == prevLine || currLine == prevLine + 1;
            }

            if (languageService.IsKind(prev, LanguageAgnosticSyntaxKind.MultiLineCommentTrivia) &&
                languageService.IsKind(current, LanguageAgnosticSyntaxKind.MultiLineCommentTrivia))
            {
                return prev.Span.End + 1 >= current.Span.Start;
            }

            return false;
        }

        private static bool SyntaxTriviaAreSame(SyntaxTrivia? a, SyntaxTrivia b)
        {
            return a.HasValue && a.Value == b;
        }

        public bool NotifyNonCommentTrivia(SyntaxTrivia trivia)
        {
            if (!hasStarted || shouldStopMatching || hasCompleted)
                return !MatchSessionEnded;

            if (lastMatchedTrivia is null)
                return !MatchSessionEnded;

            if (trivia.SyntaxTree != lastMatchedTrivia.Value.SyntaxTree)
            {
                isContiguous = false;
                shouldStopMatching = true;
                return !MatchSessionEnded;
            }

            SyntaxTree tree = trivia.SyntaxTree!;
            int lastLine = tree.GetLineSpan(lastMatchedTrivia.Value.Span).EndLinePosition.Line;
            int thisLine = tree.GetLineSpan(trivia.Span).StartLinePosition.Line;

            if (thisLine > lastLine + 1)
            {
                // Non-comment trivia on a line that skips ahead — definitely not contiguous.
                isContiguous = false;
                shouldStopMatching = true;
            }
            else if (thisLine == lastLine + 1)
            {
                // One line apart — check if the trivia is acceptable
                if (languageService.IsKind(trivia, LanguageAgnosticSyntaxKind.WhitespaceTrivia))
                {
                    // Allow whitespace between indented single-line comments
                    return !MatchSessionEnded;
                }

                // Other non-comment trivia breaks contiguity
                isContiguous = false;
                shouldStopMatching = true;
            }

            // If thisLine <= lastLine, it's trivia on the same line or before — we can ignore it.

            return !MatchSessionEnded;
        }

        /// <summary>
        /// Gets whether all lines were matched in order without gaps.
        /// </summary>
        public bool IsFullMatch => hasCompleted && isContiguous;

        /// <summary>
        /// Gets whether matching started and proceeded in order, but was incomplete.
        /// </summary>
        public bool IsPartialMatch => hasStarted && !hasCompleted && isContiguous;

        /// <summary>
        /// Gets whether the lines appear sequentially and in order without any gaps
        /// in the code file.
        /// </summary>
        public bool IsMatchContiguous => isContiguous;

        /// <summary>
        /// True when the session has encountered a line that definitively disqualifies it
        /// from matching further lines. This does not necessarily indicate a full mismatch,
        /// only that this session should not be fed additional lines.
        /// </summary>
        public bool MatchSessionEnded => shouldStopMatching || hasCompleted;

        public int MatchedLineCount => matchedLineCount;

        public IReadOnlyList<string> Lines => expectedLines;

        /// <summary>
        /// Gets the configuration root directory this license header applies to.
        /// This can be used to check whether a code file's SPDX-License-Identifier
        /// or its file path is not valid for the current license header text.
        /// </summary>
        public string MatchDirectoryPath => matchDirectoryPath;


        public TextSpan? FullMatchSpan =>
            (startOffset >= 0 && endOffset > startOffset)
                ? TextSpan.FromBounds(startOffset, endOffset)
                : null;

        public TextSpan? PartialMismatchSpan
        {
            get
            {
                if (!IsPartialMatch || mismatchStartOffset is null || lastTriviaChecked is null)
                    return null;

                return TextSpan.FromBounds(
                    mismatchStartOffset.Value,
                    lastTriviaChecked.Value.FullSpan.End
                );
            }
        }
    }
}
