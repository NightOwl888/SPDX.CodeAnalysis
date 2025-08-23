// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SPDX.CodeAnalysis
{
    public sealed class LicenseHeaderCache
    {
        private readonly Dictionary<StringKey, FallbackTreeNode> _spdxTrees = new();
        private readonly List<LicenseHeaderCacheText> _allLicenseHeaders = new();

        public LicenseHeaderCache(IEnumerable<LicenseHeaderCacheText> licenseHeaderTexts)
        {
            if (licenseHeaderTexts is null)
                throw new ArgumentNullException(nameof(licenseHeaderTexts));

            Load(licenseHeaderTexts);
        }

        public IReadOnlyList<LicenseHeaderCacheText> GetMatchingLicenseHeaders(ReadOnlySpan<char> spdxLicenseIdentifier, string codeFilePath)
        {
            if (codeFilePath is null)
                throw new ArgumentNullException(nameof(codeFilePath));

            if (!TryGetValue(spdxLicenseIdentifier, out FallbackTreeNode root))
                return Empty;

            return root.FindBestMatch(codeFilePath)?.Headers ?? Empty;
        }

        public IReadOnlyList<LicenseHeaderCacheText> GetAllLicenseHeaders() => _allLicenseHeaders;

        public IEnumerable<string> GetAllSpdxLicenseIdentifiers()
            => _spdxTrees.Keys.Select(k => k.ToString());

        public enum MatchResult
        {
            Success,
            NonMatchingSpdxIdentifier,
            NonMatchingCodeFilePath,
        }

        /// <summary>
        /// Determines whether the supplied configuration license header <paramref name="licenseHeaderMatchDirectoryPath"/> is a match for both the
        /// <paramref name="codeFileSpdxLicenseIdentifier"/> and <paramref name="codeFilePath"/>.
        /// </summary>
        /// <param name="licenseHeaderMatchDirectoryPath">The directory path that represents the configuration root of a license header text file.</param>
        /// <param name="codeFileSpdxLicenseIdentifier">The SPDX-License-Identifier tag value parsed from the code file.</param>
        /// <param name="codeFilePath">The absolute path to the code file.</param>
        /// <param name="result">A value indicating success or the type of match failure that occurred.</param>
        /// <returns><c>true</c> if the match was successful, otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryMatch(string licenseHeaderMatchDirectoryPath, ReadOnlySpan<char> codeFileSpdxLicenseIdentifier, string codeFilePath, out MatchResult result)
        {
            if (licenseHeaderMatchDirectoryPath is null)
                throw new ArgumentNullException(nameof(licenseHeaderMatchDirectoryPath));
            if (codeFilePath is null)
                throw new ArgumentNullException(nameof(codeFilePath));

            if (!TryGetValue(codeFileSpdxLicenseIdentifier, out FallbackTreeNode root))
            {
                result = MatchResult.NonMatchingSpdxIdentifier;
                return false;
            }

            FallbackTreeNode? best = root.FindBestMatch(codeFilePath);
            if (best is null || !best.MatchDirectoryPath.Equals(licenseHeaderMatchDirectoryPath, StringComparison.Ordinal))
            {
                result = MatchResult.NonMatchingCodeFilePath;
                return false;
            }

            result = MatchResult.Success;
            return true;
        }

        private sealed class FallbackTreeNode
        {
            private readonly List<LicenseHeaderCacheText> _headers = new();
            private readonly List<FallbackTreeNode> _children = new();
            public string MatchDirectoryPath { get; }

            public FallbackTreeNode(string matchDirectoryPath)
            {
                MatchDirectoryPath = matchDirectoryPath;
            }

            public IReadOnlyList<LicenseHeaderCacheText> Headers => _headers;

            public void Add(LicenseHeaderCacheText header)
            {
                // If this node matches the header's MatchDirectoryPath exactly, add here.
                if (string.Equals(header.MatchDirectoryPath, MatchDirectoryPath, StringComparison.Ordinal))
                {
                    _headers.Add(header);
                    return;
                }

                // Try to find a child whose MatchDirectoryPath is the prefix of the header's MatchDirectoryPath
                var matchingChild = _children
                    .Where(c => header.MatchDirectoryPath.StartsWith(c.MatchDirectoryPath, StringComparison.Ordinal))
                    .OrderByDescending(c => c.MatchDirectoryPath.Length)
                    .FirstOrDefault();

                if (matchingChild != null)
                {
                    matchingChild.Add(header);
                }
                else
                {
                    // No suitable child found — create new child node at this level
                    var newNode = new FallbackTreeNode(header.MatchDirectoryPath);
                    newNode._headers.Add(header);
                    _children.Add(newNode);
                }
            }

            public FallbackTreeNode? FindBestMatch(string absoluteCodeFilePath)
            {
                FallbackTreeNode? best = null;
                
                void Search(FallbackTreeNode node)
                {
                    if (absoluteCodeFilePath.StartsWith(node.MatchDirectoryPath, StringComparison.Ordinal))
                    {
                        if (best == null || node.MatchDirectoryPath.Length > best.MatchDirectoryPath.Length)
                        {
                            best = node;
                        }
                    }

                    foreach (var child in node._children)
                    {
                        Search(child);
                    }
                }

                Search(this);

                return best;
            }
        }

        private static readonly IReadOnlyList<LicenseHeaderCacheText> Empty = Array.Empty<LicenseHeaderCacheText>();

        public bool IsEmpty => _allLicenseHeaders.Count == 0;

        private void Load(IEnumerable<LicenseHeaderCacheText> licenseHeaderTexts)
        {
            foreach (var text in licenseHeaderTexts)
            {
                _allLicenseHeaders.Add(text);

                if (!TryGetValue(text.SpdxLicenseIdentifier.AsSpan(), out FallbackTreeNode root))
                {
                    root = new FallbackTreeNode(text.MatchDirectoryPath);
                    _spdxTrees[new StringKey(text.SpdxLicenseIdentifier)] = root;
                }

                root.Add(text);
            }
        }

        private bool TryGetValue(ReadOnlySpan<char> spdxLicenseIdentifier, out FallbackTreeNode root)
        {
            int hashCode = StringHelper.GetHashCode(spdxLicenseIdentifier);
            foreach (var kvp in _spdxTrees)
            {
                if (kvp.Key.GetHashCode() == hashCode && kvp.Key.Equals(spdxLicenseIdentifier))
                {
                    root = kvp.Value;
                    return true;
                }
            }
            root = default!;
            return false;
        }
    }
}
