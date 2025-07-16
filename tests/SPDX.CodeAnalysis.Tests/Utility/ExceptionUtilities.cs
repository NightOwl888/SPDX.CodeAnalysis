// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;

namespace SPDX.CodeAnalysis.Tests
{
    internal static class ExceptionUtilities
    {
        public static Exception Unreachable => new InvalidOperationException("This program location is thought to be unreachable.");
    }
}
