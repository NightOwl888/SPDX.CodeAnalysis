// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using SPDX.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace SPDX
{
    /// <summary>
    /// Represents a license header text document, which is ready for matching line-by-line.
    /// </summary>
    public sealed class LicenseHeaderCacheText
    {
        public LicenseHeaderCacheText(string spdxIdentifier, string fullFilePath, string matchDirectoryPath, IReadOnlyList<string> lines)
        {
            SpdxLicenseIdentifier = spdxIdentifier ?? throw new ArgumentNullException(nameof(spdxIdentifier));
            FullFilePath = fullFilePath ?? throw new ArgumentNullException(nameof(fullFilePath));
            MatchDirectoryPath = matchDirectoryPath ?? throw new ArgumentNullException(nameof(matchDirectoryPath));
            Lines = lines ?? throw new ArgumentNullException(nameof(lines));
        }

        public LicenseHeaderCacheText(LicenseHeaderFile file)
        {
            SpdxLicenseIdentifier = file.SpdxLicenseIdentifier;
            FullFilePath = file.FullFilePath;
            MatchDirectoryPath = file.MatchDirectoryPath;
            Lines = file.Content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

        public string SpdxLicenseIdentifier { get; }
        public string FullFilePath { get; }
        public string MatchDirectoryPath { get; }
        public IReadOnlyList<string> Lines { get; }
    }
}
