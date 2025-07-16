// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests.CSharp
{
    // TODO: Remove
    public class CSharpFileBuilderAdapter : IFileBuilder
    {
        private readonly CSharpFileBuilder _builder;

        public CSharpFileBuilderAdapter(NamespaceStyle style)
        {
            _builder = CSharpFileBuilder.Create(style);
        }

        public void WriteComment(FilePosition position, string content, CommentStyle style)
            => _builder.WriteComment(position, content, style);

        public override string ToString() => _builder.ToString();
    }
}
