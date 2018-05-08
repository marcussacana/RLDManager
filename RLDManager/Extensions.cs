using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLDManager {
    internal static class Extensions {
        public static byte[] GetRange(this byte[] From, uint Start, uint Lenght) {
            byte[] To = new byte[Lenght];
            for (uint i = Start; i - Start < Lenght; i++)
                To[i - Start] = From[i];
            return To;
        }
        public static uint GetDW(this byte[] Arr, uint Pos) {
            byte[] DW = new byte[] { Arr[Pos], Arr[Pos + 1], Arr[Pos + 2], Arr[Pos + 3] };
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(DW, 0, 4);
            return BitConverter.ToUInt32(DW, 0);
        }

        public static string GetStringAt(this byte[] Arr, uint At, Encoding Encoding) {
            List<byte> Buffer = new List<byte>();
            do {
                Buffer.Add(Arr[At++]);
            } while (Buffer.Last() != 0x00);
            Buffer.RemoveAt(Buffer.Count - 1);
            return Encoding.GetString(Buffer.ToArray());
        }
        public static T[] Append<T>(this T[] Array, T Content) {
            return Append(Array, new T[] { Content });
        }
        public static T[] Append<T>(this T[] Array, T[] Content) {
            T[] NewArray = new T[Array.LongLength + Content.LongLength];
            Array.CopyTo(NewArray, 0);
            Content.CopyTo(NewArray, Array.LongLength);

            return NewArray;
        }
    }
}
