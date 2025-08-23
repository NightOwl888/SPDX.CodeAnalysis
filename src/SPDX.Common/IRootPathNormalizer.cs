// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;

namespace SPDX.CodeAnalysis
{
    public interface IRootPathNormalizer
    {
        string RootPath { get; }
        string Normalize(ReadOnlySpan<char> subPath, bool ensureTrailingSlash = false);
        
        string Normalize(string path, bool ensureTrailingSlash = false);

        bool IsNormalized(ReadOnlySpan<char> path, bool ensureTrailingSlash = false);
    }
}