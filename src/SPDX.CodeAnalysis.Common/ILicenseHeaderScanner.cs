// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis
{
    public interface ILicenseHeaderScanner
    {
        bool TryScanForLicenseHeaders(SyntaxNode root, IReadOnlyList<LicenseHeaderCacheText> configuredLicenseHeaders, out IReadOnlyList<LicenseHeaderMatchSession> result);
    }
}
