// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using Microsoft.CodeAnalysis;

namespace SPDX.CodeAnalysis
{
    public interface ITagValueScanner
    {
        bool ScanForTagAndValue(SyntaxNode root,
            ref TagValueSession spdxLicenseIdentifierSession,
            ref TagValueSession spdxFileCopyrightTextSession);
    }
}
