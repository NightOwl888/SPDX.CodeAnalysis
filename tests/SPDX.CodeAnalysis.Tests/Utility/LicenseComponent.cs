// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;

namespace SPDX.CodeAnalysis.Tests
{
    // TODO: Remove this
    [Flags]
    public enum LicenseComponent
    {
        None = 0,
        LicenseIdentifier = 1 << 0,
        CopyrightText = 1 << 1,
        LicenseText = 1 << 2
    }
}
