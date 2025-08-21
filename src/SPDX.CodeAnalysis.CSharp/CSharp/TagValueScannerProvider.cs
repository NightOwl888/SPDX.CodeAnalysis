// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using SPDX.CodeAnalysis.Languages;

namespace SPDX.CodeAnalysis.CSharp
{
    // TODO: Return a factory that returns the language-specific instance
    public static class TagValueScannerProvider
    {
        public static ITagValueScanner Instance = new TagValueScanner(CSharpLanguageService.Instance);
    }
}
