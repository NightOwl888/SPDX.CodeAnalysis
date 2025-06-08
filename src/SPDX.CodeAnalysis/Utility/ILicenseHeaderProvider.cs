using System;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis
{
    public interface ILicenseHeaderProvider
    {
        bool TryGetLicenseHeader(string fileDirectory, ReadOnlySpan<char> spdxLicenseIdentifier, out IReadOnlyList<string> result);
    }
}
