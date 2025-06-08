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
        private static ILicenseHeaderProvider _provider = new FileSystemLicenseHeaderProvider();

        public static void SetProvider(ILicenseHeaderProvider provider) =>
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        public static void ResetOverride() => _provider = new FileSystemLicenseHeaderProvider();

        public static ILicenseHeaderProvider GetProvider() => _provider;


        //public static ILicenseHeaderProvider GetProvider(AnalyzerOptions options, SyntaxTree tree, CancellationToken token)
        //{
        //    if (_provider is not null)
        //        return _provider;

        //    var sourcePath = Path.GetDirectoryName(tree.FilePath)!;
        //    return new FileSystemLicenseHeaderProvider(sourcePath, options);
        //}
    }
}
