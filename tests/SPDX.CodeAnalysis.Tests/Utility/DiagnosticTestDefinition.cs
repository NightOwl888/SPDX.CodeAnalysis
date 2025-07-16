// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE.txt file or at https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPDX.CodeAnalysis.Tests
{
    // TODO: Remove this
    public abstract class DiagnosticTestDefinition
    {
        public abstract string DiagnosticId { get; }
        public abstract IEnumerable<DiagnosticTestCase> GetTestCases(CodeStyleCombination style);
    }
}
