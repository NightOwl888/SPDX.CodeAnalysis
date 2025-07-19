using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// A state machine that tracks the state related to a specific tag/value pair within a trivia section.
    /// </summary>
    public ref struct TagValueSession
    {
        private /*readonly*/ string tag;
        private bool hasTag;
        private bool hasValue;
        private string? containingText;

        // Store the start/length of the current line elements relative to the
        // beginning of the containing text rather than relative to the file start.
        private int tagOffsetInText;
        private int tagLength;

        private int delimiterOffsetInText;
        private int delimiterLength;

        private int valueOffsetInText;
        private int valueLength;

        private int fullOffsetInText;
        private int fullLength;

        public TagValueSession(string tag)
        {
            this.tag = tag ?? throw new ArgumentNullException(nameof(tag));
            hasTag = false;
            hasValue = false;
            containingText = null;
            valueOffsetInText = 0;
            valueLength = 0;
            tagOffsetInText = 0;
            tagLength = tag.Length;
            fullOffsetInText = 0;
            fullLength = 0;
        }

        public bool HasTag => hasTag;
        public bool HasValue => hasValue;

        public ReadOnlySpan<char> Tag =>
            containingText is null ? default : containingText.AsSpan(tagOffsetInText, tagLength);

        public ReadOnlySpan<char> Delimiter =>
            containingText is null ? default : containingText.AsSpan(delimiterOffsetInText, delimiterLength);

        public ReadOnlySpan<char> Value =>
            containingText is null ? default : containingText.AsSpan(valueOffsetInText, valueLength);

        public ReadOnlySpan<char> FullMatch =>
            containingText is null ? default : containingText.AsSpan(fullOffsetInText, fullLength);

        public int TagOffsetInText => tagOffsetInText;
        public int TagLength => tagLength;

        public int ValueOffsetInText => valueOffsetInText;
        public int ValueLength => valueLength;

        public int DelimiterOffsetInText => delimiterOffsetInText;
        public int DelimiterLength => delimiterLength;

        public int FullOffsetInText => fullOffsetInText;
        public int FullLength => fullLength;

        /// <summary>
        /// Attempts to locate the tag and its value within a single line of trivia text.
        /// </summary>
        /// <param name="trivia">The trivia the line comes from (not currently used).</param>
        /// <param name="containingText">The full text containing the trivia.</param>
        /// <param name="line">The current line being scanned.</param>
        /// <param name="lineOffset">The offset of the start of the line within the full trivia text.</param>
        /// <returns>True if the tag was found on this line.</returns>
        public bool TryFindTag(SyntaxTrivia trivia, string containingText, ReadOnlySpan<char> line, int lineOffset = 0)
        {
            int offset = line.IndexOf(tag.AsSpan(), StringComparison.Ordinal);
            if (offset > -1)
            {
                this.containingText = containingText;
                hasTag = true;

                // Absolute offset from the start of the full trivia string
                tagOffsetInText = lineOffset + offset;

                // Delimiter: should be ": " (colon and space) immediately after tag
                int delimiterStartInLine = offset + tagLength;
                ReadOnlySpan<char> lineAfterTag = line.Slice(delimiterStartInLine);
                delimiterLength = 0;
                delimiterOffsetInText = lineOffset + delimiterStartInLine;

                // Absolute offset from the start of the full trivia string
                if (lineAfterTag.StartsWith(": ".AsSpan(), StringComparison.Ordinal))
                {
                    delimiterLength = 2;
                }
                else if (lineAfterTag.StartsWith(":".AsSpan(), StringComparison.Ordinal))
                {
                    delimiterLength = 1;
                }

                // Value starts after the delimiter
                int valueOffsetInLine = delimiterStartInLine + delimiterLength;

                // Absolute offset from the start of the full trivia string
                valueOffsetInText = lineOffset + valueOffsetInLine;
                valueLength = Math.Max(line.Length - valueOffsetInLine, 0);

                // Absolute offset from the start of the full trivia string
                // Full match includes from tag start to end of line
                fullOffsetInText = tagOffsetInText;
                fullLength = Math.Max(line.Length - offset, 0); // from tag start to line end

                // No trimming — we want to verify if it's literally empty
                // Use REUSE or another SPDX validation tool to get more accurate validation.
                hasValue = valueLength > 0 && !containingText.AsSpan(valueOffsetInText, valueLength).IsWhiteSpace();

                return true; // We assume the tag won't span more than one line - this is a limitation of the design
            }

            return false;
        }
    }
}
