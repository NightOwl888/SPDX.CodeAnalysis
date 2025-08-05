// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading;

namespace SPDX.CodeAnalysis
{
    public interface ILicenseHeaderCacheLifetimeManager
    {
        LicenseHeaderCache GetCache(ImmutableArray<AdditionalText> additionalFiles, string topLevelDirectoryName, CancellationToken cancellationToken = default);
    }
}
