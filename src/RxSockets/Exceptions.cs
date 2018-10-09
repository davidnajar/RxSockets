using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace RxSockets
{
    public static class PipelineExtensions
    {
        private const int _maxULongByteLength = 20;

        [ThreadStatic]
        private static byte[] _numericBytesScratch;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ToSpan(this ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return buffer.First.Span;
            }
            return buffer.ToArray();
        }

        public static ArraySegment<byte> GetArray(this Memory<byte> buffer)
        {
            return ((ReadOnlyMemory<byte>)buffer).GetArray();
        }

        public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }
            return result;
        }


        private static byte[] NumericBytesScratch => _numericBytesScratch ?? CreateNumericBytesScratch();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static byte[] CreateNumericBytesScratch()
        {
            var bytes = new byte[_maxULongByteLength];
            _numericBytesScratch = bytes;
            return bytes;
        }

       
     

    }
    public static class StringUtilities
    {
        public static string UnsafeAsciiBytesToString(this byte[] buffer, int offset, int length)
        {
            unsafe
            {
                fixed (byte* pAscii = buffer)
                {
                    return new String((sbyte*)pAscii, offset, length);
                }
            }
        }
 
        private static readonly string _encode16Chars = "0123456789ABCDEF";

        /// <summary>
        /// A faster version of String.Concat(<paramref name="str"/>, <paramref name="separator"/>, <paramref name="number"/>.ToString("X8"))
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static unsafe string ConcatAsHexSuffix(string str, char separator, uint number)
        {
            var length = 1 + 8;
            if (str != null)
            {
                length += str.Length;
            }

            // stackalloc to allocate array on stack rather than heap
            char* charBuffer = stackalloc char[length];

            var i = 0;
            if (str != null)
            {
                for (i = 0; i < str.Length; i++)
                {
                    charBuffer[i] = str[i];
                }
            }

            charBuffer[i] = separator;

            charBuffer[i + 1] = _encode16Chars[(int)(number >> 28) & 0xF];
            charBuffer[i + 2] = _encode16Chars[(int)(number >> 24) & 0xF];
            charBuffer[i + 3] = _encode16Chars[(int)(number >> 20) & 0xF];
            charBuffer[i + 4] = _encode16Chars[(int)(number >> 16) & 0xF];
            charBuffer[i + 5] = _encode16Chars[(int)(number >> 12) & 0xF];
            charBuffer[i + 6] = _encode16Chars[(int)(number >> 8) & 0xF];
            charBuffer[i + 7] = _encode16Chars[(int)(number >> 4) & 0xF];
            charBuffer[i + 8] = _encode16Chars[(int)number & 0xF];

            // string ctor overload that takes char*
            return new string(charBuffer, 0, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Needs a push
        private static bool CheckBytesInAsciiRange(Vector<sbyte> check)
        {
            // Vectorized byte range check, signed byte > 0 for 1-127
            return Vector.GreaterThanAll(check, Vector<sbyte>.Zero);
        }

        // Validate: bytes != 0 && bytes <= 127
        //  Subtract 1 from all bytes to move 0 to high bits
        //  bitwise or with self to catch all > 127 bytes
        //  mask off high bits and check if 0

        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Needs a push
        private static bool CheckBytesInAsciiRange(long check)
        {
            const long HighBits = unchecked((long)0x8080808080808080L);
            return (((check - 0x0101010101010101L) | check) & HighBits) == 0;
        }

        private static bool CheckBytesInAsciiRange(int check)
        {
            const int HighBits = unchecked((int)0x80808080);
            return (((check - 0x01010101) | check) & HighBits) == 0;
        }

        private static bool CheckBytesInAsciiRange(short check)
        {
            const short HighBits = unchecked((short)0x8080);
            return (((short)(check - 0x0101) | check) & HighBits) == 0;
        }

        private static bool CheckBytesInAsciiRange(sbyte check)
            => check > 0;
    }

}
