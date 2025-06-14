using System;
using System.Collections.Generic;
using System.Text;

namespace SPDX.CodeAnalysis
{
    public interface ILicenseDiscoveryStrategy
    {
        /// <summary>
        /// Finds the most appropriate LICENSES.HEADERS location and path for the given SPDX ID.
        /// Could return either a single-file or a directory containing multiple headers.
        /// </summary>
        string? FindLicenseLocation(string startingDirectory, string topLevelDirName, ReadOnlySpan<char> spdxLicenseIdentifier);
    }
}
