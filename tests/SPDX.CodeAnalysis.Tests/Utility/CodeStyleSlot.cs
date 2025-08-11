// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis.Tests
{
    public readonly struct CodeStyleSlot
    {
        public CodeStyleElement Element { get; }
        public FilePosition Position { get; }
        public CommentStyle CommentStyle { get; }

        public CodeStyleSlot(CodeStyleElement element, FilePosition position, CommentStyle commentStyle)
        {
            Element = element;
            Position = position;
            CommentStyle = commentStyle;
        }

        public bool IsEnabled => true; // Extend this later if disabling is needed

        public static IEnumerable<CodeStyleSlot> AllFor(CodeStyleElement element)
        {
            foreach (var position in Enum.GetValues<FilePosition>())
                foreach (var comment in Enum.GetValues<CommentStyle>())
                    yield return new CodeStyleSlot(element, position, comment);
        }

        public override string ToString() =>
            $"{Element}_{Position}_{CommentStyle}";
    }
}
