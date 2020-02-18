using System;
using System.Runtime.InteropServices;

namespace RxSockets
{
    public static class SocketExtensions
    {
        public static ArraySegment<byte> GetArray(this Memory<byte> memory)
        {
            return ((ReadOnlyMemory<byte>)memory).GetArray();
        }

        public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
        {
            ArraySegment<byte> arraySegment;

            if (!MemoryMarshal.TryGetArray<byte>(memory, out arraySegment))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }
            return arraySegment;
        }
    }
}
