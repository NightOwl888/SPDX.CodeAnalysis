using System;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis
{
    public interface ILicenseHeaderProvider
    {
        // topLevelDirectory == "LICENSES.HEADERS"
        bool TryGetLicenseHeader(string fileDirectory, string topLevelDirectory, ReadOnlySpan<char> spdxLicenseIdentifier, out IReadOnlyList<IReadOnlyList<string>> result);
    }
}
