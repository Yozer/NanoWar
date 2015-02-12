namespace NanoWar.GameClient
{
    using System;
    using System.Runtime.InteropServices;

    internal class RawSerializer
    {
        public static T RawDeserialize<T>(byte[] rawData, int position = 0)
        {
            var rawsize = Marshal.SizeOf(typeof(T));
            if (rawsize > rawData.Length - position)
            {
                throw new ArgumentException(
                    "Not enough data to fill struct. Array length from position: " + (rawData.Length - position)
                    + ", Struct length: " + rawsize);
            }

            var buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            var retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
            Marshal.FreeHGlobal(buffer);
            return retobj;
        }

        public static byte[] RawSerialize(object item)
        {
            var rawSize = Marshal.SizeOf(item);
            var buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(item, buffer, false);
            var rawDatas = new byte[rawSize];
            Marshal.Copy(buffer, rawDatas, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawDatas;
        }
    }
}