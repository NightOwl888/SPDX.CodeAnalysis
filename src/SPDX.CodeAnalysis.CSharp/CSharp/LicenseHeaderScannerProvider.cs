// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

namespace SPDX.CodeAnalysis.CSharp
{
    public static class LicenseHeaderScannerProvider
    {
        public static ILicenseHeaderScanner Instance = new LicenseHeaderScanner(CSharpLanguageService.Instance);
    }
}
