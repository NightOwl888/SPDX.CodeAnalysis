using System;
using System.Collections.Generic;
using System.Text;

namespace SPDX.CodeAnalysis
{
    public interface ILicenseHeaderCacheLoader
    {
        IEnumerable<LicenseHeaderCacheText> LoadLicenseHeaders(string topLevelDirectoryName);
    }
}
