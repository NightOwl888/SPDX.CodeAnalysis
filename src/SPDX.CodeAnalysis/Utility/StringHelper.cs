using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SPDX.CodeAnalysis
{
    internal class StringHelper
    {
        private const int CharStackBufferSize = 64;

        public unsafe static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1)
        {
#if FEATURE_STRING_CONCAT_READONLYSPAN
            return string.Concat(str0, str1);
#else
            int length = str0.Length + str1.Length;
            if (length == 0)
            {
                return string.Empty;
            }

            bool usePool = length > CharStackBufferSize;
            char[]? arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(length) : null;
            try
            {
                Span<char> buffer = usePool ? arrayToReturnToPool : stackalloc char[length];

                str0.CopyTo(buffer.Slice(0, str0.Length));
                str1.CopyTo(buffer.Slice(str0.Length));

                fixed (char* pBuffer = buffer)
                    return new string(pBuffer, startIndex: 0, length);
            }
            finally
            {
                if (arrayToReturnToPool is not null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
#endif
        }

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
