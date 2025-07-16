using System;
using System.Collections.Generic;
using System.Text;

namespace SPDX.CodeAnalysis
{
    public ref struct LicenseHeaderMatchSession
    {
        private readonly IReadOnlyList<string> _expectedLines;
        private readonly int _lineCount;
        private int _currentLine;

        public bool IsComplete => _currentLine >= _lineCount;
        public bool HasMatchedAny => _currentLine > 0;

        public LicenseHeaderMatchSession(IReadOnlyList<string> expectedLines)
        {
            _expectedLines = expectedLines ?? throw new ArgumentNullException(nameof(expectedLines));
            _lineCount = _expectedLines.Count;
            _currentLine = 0;
        }

        public bool MatchNextLine(ReadOnlySpan<char> line)
        {
            if (_currentLine >= _lineCount)
                return true;

            ReadOnlySpan<char> expected = _expectedLines[_currentLine].AsSpan();

            int offset = line.IndexOf(expected, StringComparison.Ordinal);
            if (offset > -1)
            {
                _currentLine++;
                if (_currentLine >= _lineCount)
                    return true;
            }

            // Mismatch
            return false;
        }
    }
}
