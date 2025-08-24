using System;
using System.Buffers;
using System.IO;

namespace SPDX.CodeAnalysis
{
    public static class PathHelper
    {
        private const int CharStackBufferSize = 64;

        public static string NormalizeAndCombine(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, bool ensureTrailingSlash = false)
        {
            int length = 0;
            bool hasPath1 = false, hasPath2 = false;
            foreach (var dir in path1.SplitPath())
            {
                length += dir.Segment.Length + dir.Separator.Length;
                hasPath1 = true;
            }
            
            foreach (var dir in path2.SplitPath())
            {
                length += dir.Segment.Length + dir.Separator.Length;
                hasPath2 = true;
            }

            // Pad by one more to ensure we have enough space to separate the paths
            if (hasPath1 && hasPath2)
                length++;

            // Pad by one more if we need to end with a slash
            if (ensureTrailingSlash)
                length++;

            char[]? arrayToReturnToPool = null;
            Span<char> buffer = length > CharStackBufferSize
                ? (arrayToReturnToPool = ArrayPool<char>.Shared.Rent(length)).AsSpan(0, length)
                : stackalloc char[CharStackBufferSize];
            try
            {
                bool path1EndsWithSlash = false;
                int index = 0;

                foreach (var dir in path1.SplitPath())
                {
                    int segmentLength = dir.Segment.Length;
                    if (!dir.IsRoot)
                    {
                        if (segmentLength > 0)
                        {
                            dir.Segment.CopyTo(buffer.Slice(index));
                            index += segmentLength;
                            path1EndsWithSlash = dir.Separator.Length > 0;
                            if (path1EndsWithSlash)
                            {
                                buffer[index] = Path.DirectorySeparatorChar;
                                index++;
                            }
                        }
                    }
                    else
                    {
                        // Windows-style root
                        if (PathInternal.DirectorySeparatorChar != PathInternal.AltDirectorySeparatorChar)
                        {
                            for (int i = index; i < segmentLength; i++)
                            {
                                char c = dir.Segment[i];
                                // The alternate char is never valid in a Windows root, so we always replace it
                                if (c == PathInternal.AltDirectorySeparatorChar)
                                    c = PathInternal.DirectorySeparatorChar;

                                buffer[index] = c;
                                index++;
                            }
                            path1EndsWithSlash = dir.Separator.Length > 0;
                            if (path1EndsWithSlash)
                            {
                                buffer[index] = PathInternal.DirectorySeparatorChar;
                                index++;
                            }
                        }
                        else // Unix-style root
                        {
                            // The IsRoot flag is enough to indicate we need to write a single /
                            buffer[index] = Path.DirectorySeparatorChar;
                            index++;
                            path1EndsWithSlash = true;
                        }
                    }
                }

                if (!hasPath2)
                {
                    if (ensureTrailingSlash && !path1EndsWithSlash)
                    {
                        buffer[index] = Path.DirectorySeparatorChar;
                        index++;
                    }
                    
                    return buffer.Slice(0, index).ToString();
                }

                if (!path1EndsWithSlash)
                {
                    buffer[index] = Path.DirectorySeparatorChar;
                    index++;
                }

                bool path2EndsWithSlash = false;
                
                foreach (var dir in path2.SplitPath())
                {
                    if (dir.Segment.Length > 0)
                    {
                        dir.Segment.CopyTo(buffer.Slice(index));
                        index += dir.Segment.Length;
                        path2EndsWithSlash = dir.Separator.Length > 0;
                        if (path2EndsWithSlash)
                        {
                            buffer[index] = Path.DirectorySeparatorChar;
                            index++;
                        }
                    }
                }

                if (ensureTrailingSlash && !path2EndsWithSlash)
                {
                    buffer[index] = Path.DirectorySeparatorChar;
                    index++;
                }

                return buffer.Slice(0, index).ToString();
            }
            finally
            {
                if (arrayToReturnToPool is not null)
                {
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
                }
            }
        }

        /// <summary>
        /// The returned ReadOnlySpan contains the characters of the path that follows the last separator in path.
        /// </summary>
        public static ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
        {
            int root = PathInternal.GetPathRoot(path).Length;

            // We don't want to cut off "C:\file.txt:stream" (i.e. should be "file.txt:stream")
            // but we *do* want "C:Foo" => "Foo". This necessitates checking for the root.

            int i = PathInternal.DirectorySeparatorChar == PathInternal.AltDirectorySeparatorChar ?
                path.LastIndexOf(PathInternal.DirectorySeparatorChar) :
                path.LastIndexOfAny(PathInternal.DirectorySeparatorChar, PathInternal.AltDirectorySeparatorChar);

            return path.Slice(i < root ? root : i + 1);
        }

        /// <summary>
        /// Returns the directory portion of a file path. The returned value is empty
        /// if the specified path is null, empty, or a root (such as "\", "C:", or
        /// "\\server\share").
        /// </summary>
        /// <remarks>
        /// Unlike the string overload, this method will not normalize directory separators.
        /// </remarks>
        public static ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
        {
            if (PathInternal.IsEffectivelyEmpty(path))
                return ReadOnlySpan<char>.Empty;

            int end = GetDirectoryNameOffset(path);
            return end >= 0 ? path.Slice(0, end) : ReadOnlySpan<char>.Empty;
        }

        internal static int GetDirectoryNameOffset(ReadOnlySpan<char> path)
        {
            int rootLength = PathInternal.GetRootLength(path);
            int end = path.Length;
            if (end <= rootLength)
                return -1;

            while (end > rootLength && !PathInternal.IsDirectorySeparator(path[--end])) ;

            // Trim off any remaining separators (to deal with C:\foo\\bar)
            while (end > rootLength && PathInternal.IsDirectorySeparator(path[end - 1]))
                end--;

            return end;
        }
    }
}
