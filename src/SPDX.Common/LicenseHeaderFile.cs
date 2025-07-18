using System;
using System.Collections.Generic;
using System.Text;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// Represents a license header text file and its contents.
    /// </summary>
    public sealed class LicenseHeaderFile
    {
        public LicenseHeaderFile(string spdxIdentifier, string fullFilePath, string matchDirectoryPath, string content)
        {
            SpdxLicenseIdentifier = spdxIdentifier ?? throw new ArgumentNullException(nameof(spdxIdentifier));
            FullFilePath = fullFilePath ?? throw new ArgumentNullException(nameof(fullFilePath));
            MatchDirectoryPath = matchDirectoryPath ?? throw new ArgumentNullException(nameof(matchDirectoryPath));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public string SpdxLicenseIdentifier { get; }
        public string FullFilePath { get; }
        public string MatchDirectoryPath { get; }
        public string Content { get; }
    }
}
