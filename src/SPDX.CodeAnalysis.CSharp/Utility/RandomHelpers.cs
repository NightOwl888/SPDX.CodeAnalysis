using System;
using System.Buffers;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SPDX.CodeAnalysis
{
    internal static class RandomHelpers
    {
        // Mimics Interop.GetRandomBytes(byte* buffer, int size) using RandomNumberGenerator
        internal static unsafe void GetRandomBytes(byte* buffer, int size)
        {
//#if FEATURE_RANDOMNUMBERGENERATOR_FILL_SPAN
//            Span<byte> span = new Span<byte>(buffer, size);
//            RandomNumberGenerator.Fill(span);
//#elif FEATURE_RANDOMNUMBERGENERATOR_GETBYTES_OFFSET_COUNT
            byte[]? rentedArray = null;

            try
            {
                // Rent a buffer from the ArrayPool
                rentedArray = ArrayPool<byte>.Shared.Rent(size);

                // Fill the rented buffer with random bytes
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(rentedArray, 0, (int)size);
                }

                // Efficiently copy the random bytes to the provided buffer
                fixed (byte* source = rentedArray)
                {
                    Buffer.MemoryCopy(source, buffer, size, size);
                }
            }
            finally
            {
                if (rentedArray != null)
                {
                    // Return the array to the pool
                    ArrayPool<byte>.Shared.Return(rentedArray);
                }
            }
//#else
//            byte[] tempArray = new byte[size];

//            // Fill the tempArray with random bytes
//            using (var rng = RandomNumberGenerator.Create())
//            {
//                rng.GetBytes(tempArray);
//            }

//            // Manually copy each byte to the buffer
//            for (int i = 0; i < size; i++)
//            {
//                buffer[i] = tempArray[i];
//            }
//#endif
        }
    }
}
