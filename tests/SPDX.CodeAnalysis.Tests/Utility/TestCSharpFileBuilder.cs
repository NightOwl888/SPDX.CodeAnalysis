using NUnit.Framework;
using System;

namespace SPDX.CodeAnalysis.Tests
{
    public class TestCSharpFileBuilder
    {
        [Test]
        public void Test_GlobalNamespace_WithType()
        {
            var builder = CSharpFileBuilder.Create(NamespaceStyle.Global)
                .BeforeUsings("// SPDX-License-Identifier: Apache-2.0")
                .BeforeUsings("// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith")
                .BeforeType("// Apache 2.0 License Text");

            var result = builder.ToString();

            string expected = @"
// SPDX-License-Identifier: Apache-2.0
// SPDX-FileCopyrightText: Copyright 2025-2028 John Smith

using System;
using System.Collections.Generic;
using System.Text;

// Apache 2.0 License Text

public class MyClass
{
    public void MyMethod()
    {
    }
}
".TrimStart();

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Test_BlockScopedNamespace_WithoutType()
        {
            var builder = CSharpFileBuilder.Create(NamespaceStyle.BlockScoped)
                .BeforeUsings("// SPDX-License-Identifier: Apache-2.0")
                .BeforeNamespace("// Apache 2.0 License Text")
                .BeforeType("// Before type")
                .WithoutType();

            var result = builder.ToString();

            string expected = @"
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text;

// Apache 2.0 License Text

namespace MyNamespace
{
    // Before type
}
".TrimStart();

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Test_FileScopedNamespace_WithType()
        {
            var builder = CSharpFileBuilder.Create(NamespaceStyle.FileScoped)
                .BeforeUsings("// SPDX-License-Identifier: Apache-2.0")
                .BeforeNamespace("// Apache License")
                .BeforeType("// Before type");

            var result = builder.ToString();

            string expected = @"
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text;

// Apache License

namespace MyNamespace;

// Before type

public class MyClass
{
    public void MyMethod()
    {
    }
}
".TrimStart();

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Test_FileScopedNamespace_WithoutType_AndNulls()
        {
            var builder = CSharpFileBuilder.Create(NamespaceStyle.FileScoped)
                .BeforeUsings(null)
                .BeforeNamespace(null)
                .BeforeType(null)
                .WithoutType();

            var result = builder.ToString();

            string expected = @"

using System;
using System.Collections.Generic;
using System.Text;

namespace MyNamespace;

".TrimStart();

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Test_FileScopedNamespace_WithType_AndNulls()
        {
            var builder = CSharpFileBuilder.Create(NamespaceStyle.FileScoped)
                .BeforeUsings(null)
                .BeforeNamespace(null)
                .BeforeType(null);

            var result = builder.ToString();

            string expected = @"

using System;
using System.Collections.Generic;
using System.Text;

namespace MyNamespace;


public class MyClass
{
    public void MyMethod()
    {
    }
}
".TrimStart();

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
