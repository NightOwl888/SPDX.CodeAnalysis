using System;
using System.Collections;
using System.Collections.Generic;

namespace SPDX.CodeAnalysis
{
    public sealed class StringKey
    {
        private readonly string _spdxIdentifier;
        private readonly string _licenseLocation;

        public StringKey(string spdxIdentifier, string licenseLocation)
        {
            _spdxIdentifier = spdxIdentifier ?? throw new ArgumentNullException(nameof(spdxIdentifier));
            _licenseLocation = licenseLocation ?? throw new ArgumentNullException(nameof(licenseLocation));
        }

        public bool Equals(ReadOnlySpan<char> spdxIdentifier, ReadOnlySpan<char> licenseLocation)
            => _spdxIdentifier.AsSpan().SequenceEqual(spdxIdentifier) && _licenseLocation.AsSpan().SequenceEqual(licenseLocation);

        public override bool Equals(object obj)
        {
            if (obj is StringKey str)
                return Comparer.Equals(this, str);

            return false;
        }

        public override int GetHashCode()
            => GetHashCode(_spdxIdentifier.AsSpan(), _licenseLocation.AsSpan());

        public static int GetHashCode(ReadOnlySpan<char> spdxIdentifier, ReadOnlySpan<char> licenseLocation)
        {
            int hash = 17;
            hash = hash * 31 + StringHelper.GetHashCode(spdxIdentifier);
            hash = hash * 31 + StringHelper.GetHashCode(licenseLocation);
            return hash;
        }

        public static KeyComparer Comparer { get; } = new KeyComparer();

        public sealed class KeyComparer : IEqualityComparer<StringKey>
        {
            public bool Equals(StringKey? x, StringKey? y)
            {
                if (x is null)
                    return y is null;
                if (y is null)
                    return false;

                if (!x._spdxIdentifier.AsSpan().SequenceEqual(y._spdxIdentifier.AsSpan()))
                    return false;
                if (!x._licenseLocation.AsSpan().SequenceEqual(y._licenseLocation.AsSpan()))
                    return false;

                return true;
            }

            public int GetHashCode(StringKey obj)
                => obj.GetHashCode();
        }
    }
}
