// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.IO;

namespace SPDX.CodeAnalysis
{
    public class RootPathNormalizer : IRootPathNormalizer
    {
        private readonly string rootPath;

        public RootPathNormalizer(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path must not be null or empty.", nameof(path));

            rootPath = Path.IsPathRooted(path)
                ? PathHelper.NormalizeAndCombine(path.AsSpan(), ReadOnlySpan<char>.Empty)
                : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        }

        public string RootPath => rootPath;

        /// <summary>
        /// Normalizes the given path, fixing the slashes to match the current OS.
        /// If it is not rooted, the <see cref="RootPath"/> will be prepended. Otherwise,
        /// only the slashes will be corrected.
        /// <para/>
        /// This method always performs a heap allocation. To avoid this, either ensure
        /// you pass a relative path or call <see cref="IsNormalized(ReadOnlySpan{Char}, bool)"/>
        /// before calling this method. Alternatively, call the <see cref="Normalize(string, bool)"/>
        /// overload, which returns the original string if normalization is unnecessary.
        /// </summary>
        /// <param name="path">The relative or full path to normalize.</param>
        /// <param name="ensureTrailingSlash">If <c>true</c>, ensures there will be a trailing
        /// slash at the end of the result. Generally, this is only useful for directory paths.</param>
        /// <returns>The normalized path.</returns>
        public string Normalize(ReadOnlySpan<char> path, bool ensureTrailingSlash = false)
        {
            ReadOnlySpan<char> rootPathSpan = rootPath.AsSpan();

            if (PathInternal.IsPathRooted(path))
            {
                return PathHelper.NormalizeAndCombine(path, ReadOnlySpan<char>.Empty, ensureTrailingSlash);
            }

            return PathHelper.NormalizeAndCombine(rootPathSpan, path, ensureTrailingSlash);
        }
        
        public string Normalize(string path, bool ensureTrailingSlash = false)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            ReadOnlySpan<char> pathSpan = path.AsSpan();
            
            // Fast-path: if already normalized, return the same reference
            if (IsNormalized(pathSpan, ensureTrailingSlash))
                return path;

            return Normalize(pathSpan, ensureTrailingSlash);
        }

        public bool IsNormalized(ReadOnlySpan<char> path, bool ensureTrailingSlash = false)
        {
            if (!PathInternal.IsPathRooted(path))
                return false;

            // Check for incorrect slashes (note that on Unix/macOS, \ is a valid path character).
            if (PathInternal.DirectorySeparatorChar != PathInternal.AltDirectorySeparatorChar &&
                path.IndexOf(PathInternal.AltDirectorySeparatorChar) >= 0)
                return false;

            if (ensureTrailingSlash && path[path.Length - 1] != Path.DirectorySeparatorChar)
                return false;

            // If we got here, it's normalized
            return true;
        }
    }
}