using System;
using System.Runtime.CompilerServices;

namespace SPDX.CodeAnalysis
{
    public static class MemoryExtensions
    {
        /// <summary>
        /// Determines whether the specified value appears at the start of the span.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to compare.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>? =>
            span.Length != 0 && (span[0]?.Equals(value) ?? (object?)value is null);

        public static LineSplitEnumerator SplitLines(this ReadOnlySpan<char> str)
        {
            // LineSplitEnumerator is a struct so there is no allocation here
            return new LineSplitEnumerator(str);
        }
        public static LineSplitEnumerator SplitLines(this string str)
        {
            // LineSplitEnumerator is a struct so there is no allocation here
            return new LineSplitEnumerator(str.AsSpan());
        }

        public static PathSplitEnumerator SplitPath(this ReadOnlySpan<char> str)
        {
            // PathSplitEnumerator is a struct so there is no allocation here
            return new PathSplitEnumerator(str);
        }
        public static PathSplitEnumerator SplitPath(this string str)
        {
            // PathSplitEnumerator is a struct so there is no allocation here
            return new PathSplitEnumerator(str.AsSpan());
        }

        /// <summary>
        /// Indicates whether the specified span contains only white-space characters.
        /// </summary>
        public static bool IsWhiteSpace(this ReadOnlySpan<char> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (!char.IsWhiteSpace(span[i]))
                    return false;
            }
            return true;
        }
    }

    // Must be a ref struct as it contains a ReadOnlySpan<char>
    public ref struct LineSplitEnumerator
    {
        private ReadOnlySpan<char> _str;

        public LineSplitEnumerator(ReadOnlySpan<char> str)
        {
            _str = str;
            Current = default;
        }

        // Needed to be compatible with the foreach operator
        public LineSplitEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            var span = _str;
            if (span.Length == 0) // Reach the end of the string
                return false;

            var index = span.IndexOfAny('\r', '\n');
            if (index == -1) // The string is composed of only one line
            {
                _str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                Current = new LineSplitEntry(span, ReadOnlySpan<char>.Empty);
                return true;
            }

            if (index < span.Length - 1 && span[index] == '\r')
            {
                // Try to consume the '\n' associated to the '\r'
                var next = span[index + 1];
                if (next == '\n')
                {
                    Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 2));
                    _str = span.Slice(index + 2);
                    return true;
                }
            }

            Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 1));
            _str = span.Slice(index + 1);
            return true;
        }

        public LineSplitEntry Current { get; private set; }
    }

    public readonly ref struct LineSplitEntry
    {
        public LineSplitEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
        {
            Line = line;
            Separator = separator;
        }

        public ReadOnlySpan<char> Line { get; }
        public ReadOnlySpan<char> Separator { get; }

        // This method allow to deconstruct the type, so you can write any of the following code
        // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
        // foreach (var (line, separator) in str.SplitLines()) { _ = line; }
        // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
        public void Deconstruct(out ReadOnlySpan<char> line, out ReadOnlySpan<char> separator)
        {
            line = Line;
            separator = Separator;
        }

        // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
        // foreach (ReadOnlySpan<char> entry in str.SplitLines())
        public static implicit operator ReadOnlySpan<char>(LineSplitEntry entry) => entry.Line;
    }

    // Must be a ref struct as it contains a ReadOnlySpan<char>
    public ref struct PathSplitEnumerator
    {
        private ReadOnlySpan<char> _str;

        public PathSplitEnumerator(ReadOnlySpan<char> str)
        {
            _str = str;
            Current = default;
        }

        // Needed to be compatible with the foreach operator
        public PathSplitEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            var span = _str;
            if (span.Length == 0) // Reach the end of the string
                return false;

            // Using PathInternal ensures that on Unix/macOS we will preserve backslash characters because they are valid in file names
            var index = span.IndexOfAny(PathInternal.DirectorySeparatorChar, PathInternal.AltDirectorySeparatorChar);
            if (index == -1) // The string is composed of only one line
            {
                _str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                Current = new PathSplitEntry(span, ReadOnlySpan<char>.Empty);
                return true;
            }

            Current = new PathSplitEntry(span.Slice(0, index), span.Slice(index, 1));
            _str = span.Slice(index + 1);
            return true;
        }

        public PathSplitEntry Current { get; private set; }
    }

    public readonly ref struct PathSplitEntry
    {
        public PathSplitEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
        {
            Segment = line;
            Separator = separator;
        }

        public ReadOnlySpan<char> Segment { get; }
        public ReadOnlySpan<char> Separator { get; }

        // This method allow to deconstruct the type, so you can write any of the following code
        // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
        // foreach (var (segment, separator) in str.SplitLines()) { _ = line; }
        // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
        public void Deconstruct(out ReadOnlySpan<char> segment, out ReadOnlySpan<char> separator)
        {
            segment = Segment;
            separator = Separator;
        }

        // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
        // foreach (ReadOnlySpan<char> entry in str.SplitLines())
        public static implicit operator ReadOnlySpan<char>(PathSplitEntry entry) => entry.Segment;
    }
}
