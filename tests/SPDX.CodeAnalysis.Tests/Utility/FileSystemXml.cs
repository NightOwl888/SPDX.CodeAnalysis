// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

namespace SPDX.CodeAnalysis.Tests
{
    public abstract class FileSystemXml
    {
        public abstract string Basic { get; }
        public abstract string With1OverriddenLevel { get; }
        public abstract string With2OverriddenLevels { get; }
    }
}
