using System;
using System.Collections;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis
{
    public sealed class StringKey
    {
        private readonly string key;

        public StringKey(string key)
        {
            this.key = key;
        }

        public bool Equals(ReadOnlySpan<char> str)
            => key.AsSpan().Equals(str, StringComparison.Ordinal);

        public override bool Equals(object obj)
        {
            if (obj is StringKey other)
                return key.Equals(other.key, StringComparison.Ordinal);

            return false;
        }

        public override int GetHashCode()
            => StringHelper.GetHashCode(key.AsSpan());

        public static KeyComparer Comparer { get; } = new KeyComparer();

        public sealed class KeyComparer : IEqualityComparer<StringKey>
        {
            public bool Equals(StringKey? x, StringKey? y)
                => x?.key.Equals(y?.key, StringComparison.Ordinal) ?? false;

            public int GetHashCode(StringKey obj)
                => obj.GetHashCode();
        }
    }
}
