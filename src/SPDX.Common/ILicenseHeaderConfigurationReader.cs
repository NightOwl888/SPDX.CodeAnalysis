// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// Provides an abstraction over the configuration of license header files.
    /// </summary>
    public interface ILicenseHeaderConfigurationReader
    {
        /// <summary>
        /// Gets all of the license header configuration files using the codeFilePath as a starting point. Scans the directories up to the root
        /// to find the closest <paramref name="topLevelDirectoryName"/> to the root, then traverses all of the descendants to find all of the
        /// license header text file paths and content.
        /// </summary>
        /// <param name="codeFilePath">The starting file path of the search. Must contain a filename (even though it could be fictitious).</param>
        /// <param name="topLevelDirectoryName">The name of the configuration directory to search for.</param>
        /// <returns>An enumeration of pairs of file path and content of all configuration files within the range.</returns>
        IEnumerable<LicenseHeaderFile> GetLicenseHeaderFiles(string codeFilePath, string topLevelDirectoryName); // TODO: Change codeFilePath to startingDirectory or similar.
    }
}
