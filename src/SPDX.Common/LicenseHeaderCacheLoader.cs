using System;
using System.Collections.Generic;
using System.Text;

namespace SPDX.CodeAnalysis
{
    /// <summary>
    /// Loads a license header cache 
    /// </summary>
    public class LicenseHeaderCacheLoader : ILicenseHeaderCacheLoader
    {
        private readonly ILicenseHeaderConfigurationReader reader;
        private readonly string codeFilePath;

        public LicenseHeaderCacheLoader(ILicenseHeaderConfigurationReader reader, string codeFilePath)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.codeFilePath = codeFilePath ?? throw new ArgumentNullException(nameof(codeFilePath));
        }

        public IEnumerable<LicenseHeaderCacheText> LoadLicenseHeaders(string topLevelDirectoryName)
        {
            foreach (LicenseHeaderFile file in reader.GetLicenseHeaderFiles(codeFilePath, topLevelDirectoryName))
            {
                yield return new LicenseHeaderCacheText(file);
            }
        }
    }
}
