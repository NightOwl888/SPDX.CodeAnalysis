using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SPDX.CodeAnalysis
{
    internal class StringHelper
    {
        // A span-based equivalent of String.GetHashCode(). Computes an ordinal hash code.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<char> value)
        {
#if FEATURE_STRING_GETHASHCODE_READONLYSPAN
            return string.GetHashCode(value);
#else
            ulong seed = Marvin.DefaultSeed;

            // Multiplication below will not overflow since going from positive Int32 to UInt32.
            return Marvin.ComputeHash32(ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(value)), (uint)value.Length * 2 /* in bytes, not chars */, (uint)seed, (uint)(seed >> 32));
#endif
        }
    }
}
