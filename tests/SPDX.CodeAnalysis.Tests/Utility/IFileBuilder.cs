// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

namespace SPDX.CodeAnalysis.Tests
{
    // TODO: Remove
    public interface IFileBuilder
    {
        void WriteComment(FilePosition position, string content, CommentStyle style);
        string ToString();
    }
}
