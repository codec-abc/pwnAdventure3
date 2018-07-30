using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpForwarderClient
{
    public class ClientToServerDataAnalyzer : IDataAnalyzer
    {
        public byte[] Analyze(byte[] input)
        {
            if (input.Length >= 22)
            {
                if (input[0] == 0x6D && input[1] == 0x76)
                {
                    //Console.WriteLine("x: " + Utils.ToFloat32(input, 2 + 0, Utils.Endianness.Little));
                    //Console.WriteLine("y: " + Utils.ToFloat32(input, 2 + 4, Utils.Endianness.Little));
                    //Console.WriteLine("z: " + Utils.ToFloat32(input, 2 + 8, Utils.Endianness.Little));

                    //Console.WriteLine("u: " + Utils.ToFloat32(input, 2 + 12, Utils.Endianness.Little));
                    //Console.WriteLine("v: " + Utils.ToFloat32(input, 2 + 16, Utils.Endianness.Little));

                    //Console.WriteLine("x " + input[14]);
                    //Console.WriteLine("y " + input[15]);
                    //Console.WriteLine("z " + input[16]);
                    //Console.WriteLine("w " + input[17]);

                    //Console.WriteLine("x " + BitConverter.ToUInt16(input, 14));
                    //Console.WriteLine("y " + BitConverter.ToUInt16(input, 16));
                    Console.WriteLine("");

                }
            }
            return input;
        }
    }

    public interface IDataAnalyzer
    {
        byte[] Analyze(byte[] input);
    }

    public static class Utils
    {
        public enum Endianness
        {
            Little,
            Big
        }

        public static float ToFloat32(byte[] input, int offset, Endianness endianness)
        {
            var segment = new ArraySegment<byte>(input, offset, 4);
            byte[] arr;
            if (BitConverter.IsLittleEndian && endianness != Endianness.Little)
            {
                arr = segment.ToArray().Reverse().ToArray();

            }
            else
            {
                arr = segment.ToArray();
            }

            return BitConverter.ToSingle(arr, 0);
        }
    }
}
