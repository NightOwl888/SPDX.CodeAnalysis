// Source: https://github.com/dotnet/runtime/blob/v9.0.8/src/libraries/Common/tests/Tests/System/Text/ValueStringBuilderTests.cs
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NUnit.Framework;
using System;
using System.Text;

namespace SPDX.Common.Tests
{
    public class ValueStringBuilderTests
    {
        [Test]
        public void Ctor_Default_CanAppend()
        {
            var vsb = default(ValueStringBuilder);
            Assert.That(0, Is.EqualTo(vsb.Length));

            vsb.Append('a');
            Assert.That(vsb.Length, Is.EqualTo(1));
            Assert.That(vsb.ToString(), Is.EqualTo("a"));
        }

        [Test]
        public void Ctor_Span_CanAppend()
        {
            var vsb = new ValueStringBuilder(new char[1]);
            Assert.That(vsb.Length, Is.EqualTo(0));

            vsb.Append('a');
            Assert.That(vsb.Length, Is.EqualTo(1));
            Assert.That(vsb.ToString(), Is.EqualTo("a"));
        }

        [Test]
        public void Ctor_InitialCapacity_CanAppend()
        {
            var vsb = new ValueStringBuilder(1);
            Assert.That(vsb.Length, Is.EqualTo(0));

            vsb.Append('a');
            Assert.That(vsb.Length, Is.EqualTo(1));
            Assert.That(vsb.ToString(), Is.EqualTo("a"));
        }

        [Test]
        public void Append_Char_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (int i = 1; i <= 100; i++)
            {
                sb.Append((char)i);
                vsb.Append((char)i);
            }

            Assert.That(vsb.Length, Is.EqualTo(sb.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(sb.ToString()));
        }

        [Test]
        public void Append_String_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (int i = 1; i <= 100; i++)
            {
                string s = i.ToString();
                sb.Append(s);
                vsb.Append(s);
            }

            Assert.That(vsb.Length, Is.EqualTo(sb.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(sb.ToString()));
        }

        [TestCase(0, 4 * 1024 * 1024)]
        [TestCase(1025, 4 * 1024 * 1024)]
        [TestCase(3 * 1024 * 1024, 6 * 1024 * 1024)]
        public void Append_String_Large_MatchesStringBuilder(int initialLength, int stringLength)
        {
            var sb = new StringBuilder(initialLength);
            var vsb = new ValueStringBuilder(new char[initialLength]);

            string s = new string('a', stringLength);
            sb.Append(s);
            vsb.Append(s);

            Assert.That(vsb.Length, Is.EqualTo(sb.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(sb.ToString()));
        }

        [Test]
        public void Append_CharInt_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (int i = 1; i <= 100; i++)
            {
                sb.Append((char)i, i);
                vsb.Append((char)i, i);
            }

            Assert.That(vsb.Length, Is.EqualTo(sb.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(sb.ToString()));
        }

        [Test]
        public unsafe void Append_PtrInt_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (int i = 1; i <= 100; i++)
            {
                string s = i.ToString();
                fixed (char* p = s)
                {
                    sb.Append(p, s.Length);
                    vsb.Append(p, s.Length);
                }
            }

            Assert.That(vsb.Length, Is.EqualTo(sb.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(sb.ToString()));
        }

        [Test]
        public void AppendSpan_DataAppendedCorrectly()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();

            for (int i = 1; i <= 1000; i++)
            {
                string s = i.ToString();

                sb.Append(s);

                Span<char> span = vsb.AppendSpan(s.Length);
                Assert.That(vsb.Length, Is.EqualTo(sb.Length));

                s.AsSpan().CopyTo(span);
            }

            Assert.That(vsb.Length, Is.EqualTo(sb.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(sb.ToString()));
        }

        [Test]
        public void Insert_IntCharInt_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            var rand = new Random(42);

            for (int i = 1; i <= 100; i++)
            {
                int index = rand.Next(sb.Length);
                sb.Insert(index, new string((char)i, 1), i);
                vsb.Insert(index, (char)i, i);
            }

            Assert.That(vsb.Length, Is.EqualTo(sb.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(sb.ToString()));
        }

        [Test]
        public void AsSpan_ReturnsCorrectValue_DoesntClearBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();

            for (int i = 1; i <= 100; i++)
            {
                string s = i.ToString();
                sb.Append(s);
                vsb.Append(s);
            }

            var resultString = new string(vsb.AsSpan());
            Assert.That(resultString, Is.EqualTo(sb.ToString()));

            Assert.That(sb.Length, Is.Not.EqualTo(0));
            Assert.That(vsb.Length, Is.EqualTo(sb.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(sb.ToString()));
        }

        [Test]
        public void ToString_ClearsBuilder_ThenReusable()
        {
            const string Text1 = "test";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);
            Assert.That(vsb.Length, Is.EqualTo(Text1.Length));

            string s = vsb.ToString();
            Assert.That(s, Is.EqualTo(Text1));

            Assert.That(vsb.Length, Is.EqualTo(0));
            Assert.That(vsb.ToString(), Is.EqualTo(string.Empty));
            Assert.That(vsb.TryCopyTo(Span<char>.Empty, out _), Is.True);

            const string Text2 = "another test";
            vsb.Append(Text2);
            Assert.That(vsb.Length, Is.EqualTo(Text2.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(Text2));
        }

        [Test]
        public void TryCopyTo_FailsWhenDestinationIsTooSmall_SucceedsWhenItsLargeEnough()
        {
            var vsb = new ValueStringBuilder();

            const string Text = "expected text";
            vsb.Append(Text);
            Assert.That(vsb.Length, Is.EqualTo(Text.Length));

            Span<char> dst = new char[Text.Length - 1];
            Assert.That(vsb.TryCopyTo(dst, out int charsWritten), Is.False);
            Assert.That(charsWritten, Is.EqualTo(0));
            Assert.That(vsb.Length, Is.EqualTo(0));
        }

        [Test]
        public void TryCopyTo_ClearsBuilder_ThenReusable()
        {
            const string Text1 = "test";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);
            Assert.That(vsb.Length, Is.EqualTo(Text1.Length));

            Span<char> dst = new char[Text1.Length];
            Assert.That(vsb.TryCopyTo(dst, out int charsWritten), Is.True);
            Assert.That(charsWritten, Is.EqualTo(Text1.Length));
            Assert.That(new string(dst), Is.EqualTo(Text1));

            Assert.That(vsb.Length, Is.EqualTo(0));
            Assert.That(vsb.ToString(), Is.EqualTo(string.Empty));
            Assert.That(vsb.TryCopyTo(Span<char>.Empty, out _), Is.True);

            const string Text2 = "another test";
            vsb.Append(Text2);
            Assert.That(vsb.Length, Is.EqualTo(Text2.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(Text2));
        }

        [Test]
        public void Dispose_ClearsBuilder_ThenReusable()
        {
            const string Text1 = "test";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);
            Assert.That(vsb.Length, Is.EqualTo(Text1.Length));

            vsb.Dispose();

            Assert.That(vsb.Length, Is.EqualTo(0));
            Assert.That(vsb.ToString(), Is.EqualTo(string.Empty));
            Assert.That(vsb.TryCopyTo(Span<char>.Empty, out _), Is.True);

            const string Text2 = "another test";
            vsb.Append(Text2);
            Assert.That(vsb.Length, Is.EqualTo(Text2.Length));
            Assert.That(vsb.ToString(), Is.EqualTo(Text2));
        }

        [Test]
        public unsafe void Indexer()
        {
            const string Text1 = "foobar";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);

            Assert.That(vsb[3], Is.EqualTo('b'));
            vsb[3] = 'c';
            Assert.That(vsb[3], Is.EqualTo('c'));
            vsb.Dispose();
        }

        [Test]
        public void EnsureCapacity_IfRequestedCapacityWins()
        {
            // Note: constants used here may be dependent on minimal buffer size
            // the ArrayPool is able to return.
            var builder = new ValueStringBuilder(stackalloc char[32]);

            builder.EnsureCapacity(65);

            Assert.That(builder.Capacity, Is.EqualTo(128));
        }

        [Test]
        public void EnsureCapacity_IfBufferTimesTwoWins()
        {
            var builder = new ValueStringBuilder(stackalloc char[32]);

            builder.EnsureCapacity(33);

            Assert.That(builder.Capacity, Is.EqualTo(64));
            builder.Dispose();
        }

        [Test]
        public void EnsureCapacity_NoAllocIfNotNeeded()
        {
            // Note: constants used here may be dependent on minimal buffer size
            // the ArrayPool is able to return.
            var builder = new ValueStringBuilder(stackalloc char[64]);

            builder.EnsureCapacity(16);

            Assert.That(builder.Capacity, Is.EqualTo(64));
            builder.Dispose();
        }
    }
}