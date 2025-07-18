// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests.CSharp
{
    public sealed class CSharpFileSystemXml : FileSystemXml
    {
        public override string NoConfiguration => FileSystemXmlDefaults.NoConfiguration;
        public override string Basic => FileSystemXmlDefaults.Basic;
        public override string With1OverriddenLevel => FileSystemXmlDefaults.With1OverriddenLevel;
        public override string With2OverriddenLevels => FileSystemXmlDefaults.With2OverriddenLevels;
    }
}
