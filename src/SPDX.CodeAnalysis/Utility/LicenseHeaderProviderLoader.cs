using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SPDX.CodeAnalysis
{
    public static class LicenseHeaderProviderLoader
    {
        private static ILicenseHeaderProvider _provider = CreateDefaultLicenseHeaderProvider();

        public static void SetProvider(ILicenseHeaderProvider provider) =>
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        public static void ResetOverride() => _provider = CreateDefaultLicenseHeaderProvider();

        public static ILicenseHeaderProvider GetProvider() => _provider;


        private static ILicenseHeaderProvider CreateDefaultLicenseHeaderProvider()
        {
            var fileSystem = new FileSystem();
            return new LicenseHeaderProvider(fileSystem, new ParentDirectorySpdxDiscoveryStrategy(fileSystem));
        }
    }
}
