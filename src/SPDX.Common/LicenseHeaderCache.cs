// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SPDX.CodeAnalysis
{
    public sealed class LicenseHeaderCache
    {
        private readonly Dictionary<StringKey, FallbackTreeNode> _spdxTrees = new();
        private readonly List<LicenseHeaderCacheText> _allLicenseHeaders = new();
        private object notNullIfInitalized = null!;

        public IReadOnlyList<LicenseHeaderCacheText> GetMatchingLicenseHeaders(ReadOnlySpan<char> spdxLicenseIdentifier, string codeFilePath)
        {
            if (!TryGetValue(spdxLicenseIdentifier, out FallbackTreeNode root))
                return Empty;

            return root.FindBestMatch(codeFilePath);
        }

        public IReadOnlyList<LicenseHeaderCacheText> GetAllLicenseHeaders() => _allLicenseHeaders;

        private sealed class FallbackTreeNode
        {
            private readonly List<LicenseHeaderCacheText> _headers = new();
            private readonly List<FallbackTreeNode> _children = new();
            public string MatchDirectoryPath { get; }

            public FallbackTreeNode(string matchDirectoryPath)
            {
                MatchDirectoryPath = matchDirectoryPath;
            }

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

            public IReadOnlyList<LicenseHeaderCacheText> FindBestMatch(string codeFilePath)
            {
                FallbackTreeNode? best = null;
                string normalizedCodeFilePath = Path.GetFullPath(codeFilePath);

                void Search(FallbackTreeNode node)
                {
                    if (normalizedCodeFilePath.StartsWith(node.MatchDirectoryPath, StringComparison.Ordinal))
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

                return best?._headers ?? Empty;
            }
        }

        private static IReadOnlyList<LicenseHeaderCacheText> Empty = Array.Empty<LicenseHeaderCacheText>().ToList();

        public bool IsEmpty => _allLicenseHeaders.Count == 0;

        public void EnsureInitialized(ILicenseHeaderCacheLoader loader, string codeFilePath, string topLevelDircectoryName)
        {
            if (loader is null)
                throw new ArgumentNullException(nameof(loader));
            if (codeFilePath is null)
                throw new ArgumentNullException(nameof(codeFilePath));

            LazyInitializer.EnsureInitialized(ref notNullIfInitalized, () => {
                Load(loader.LoadLicenseHeaders(codeFilePath, topLevelDircectoryName));
                return new object();
            });
        }

        private void Load(IReadOnlyList<LicenseHeaderCacheText> licenseHeaderTexts)
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
