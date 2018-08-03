using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpForwarderClient
{
    public class ClientToServerDataAnalyzer : IDataAnalyzer
    {
        //66 74 : ft -> fast travel
        //03 00 -> Enter Location
        //73 3D -> ????
        //23 3E -> Pick dialog

        public interface ClientToServerPartReader
        {
            int GetLength();
            byte[] GetPrefix();
            void Parse(byte[] input, int offset);
        }

        class JumpReader : ClientToServerPartReader
        {
            public int GetLength()
            {
                return 3;
            }

            public byte[] GetPrefix()
            {
                return new byte[] { 0x6A, 0x70 };
            }

            public void Parse(byte[] input, int offset)
            {
            }
        }

        class MoveReader : ClientToServerPartReader
        {
            public int GetLength()
            {
                return 22;
            }

            public byte[] GetPrefix()
            {
                return new byte[] { 0x6D, 0x76 };
            }

            public void Parse(byte[] input, int offset)
            {
                var newArr = new byte[22];
                for (int i = 0; i < 22; i++)
                {
                    newArr[i] = input[i + offset];
                }

                var x = BitConverter.ToSingle(newArr, 0 + 2);
                var y = BitConverter.ToSingle(newArr, 4 + 2);
                var z = BitConverter.ToSingle(newArr, 8 + 2);

                //Console.WriteLine($"x:{x}, y:{y}, z:{z}");

                //Console.WriteLine(BitConverter.ToString(newArr));
            }
        }

        class DummyReader : ClientToServerPartReader
        {
            public int GetLength()
            {
                return 2;
            }

            public byte[] GetPrefix()
            {
                return new byte[] { 0x00, 0x00 };
            }

            public void Parse(byte[] input, int offset)
            {
            }
        }

        //class LeftClickReader : PartReader
        //{
        //    public int GetLength()
        //    {
        //        return 32; // dynamic
        //    }

        //    public byte[] GetPrefix()
        //    {
        //        return new byte[] { 0x2A, 0x69 };
        //    }
        //}

        class PickupReader : ClientToServerPartReader
        {
            public int GetLength()
            {
                return 6;
            }

            public byte[] GetPrefix()
            {
                return new byte[] { 0x65, 0x65 };
            }

            public void Parse(byte[] input, int offset)
            {
            }
        }

        public ClientToServerDataAnalyzer()
        {
            _parsers.Add(new JumpReader());
            _parsers.Add(new MoveReader());
            _parsers.Add(new DummyReader());
            //_parsers.Add(new LeftClickReader());
            //_parsers.Add(new PickupReader());

            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Common.Utils.GetLogDir(), "clientToServer"));
        }

        private List<ClientToServerPartReader> _parsers =
            new List<ClientToServerPartReader>();

        public byte[] Analyze(byte[] input, string timeAsString, string direction)
        {
            try
            {
                var index = 0;
                var canParse = true;

                while (canParse && index + 1 < input.Length)
                {
                    var byte0 = input[index + 0];
                    var byte1 = input[index + 1];

                    var offsets = _parsers
                        .Select
                        (
                            parser =>
                            {
                                var bytes = parser.GetPrefix();
                                if (bytes[0] == byte0 && bytes[1] == byte1)
                                {
                                    return new Tuple<ClientToServerPartReader, int>(parser, parser.GetLength());
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        )
                        .Where(l => l != null).ToList();

                    if (offsets.Any())
                    {
                        var parser = offsets.First();
                        parser.Item1.Parse(input, index);
                        index += parser.Item2;
                    }
                    else
                    {
                        canParse = false;
                        Console.WriteLine(Utils.GetTimeStamp() + " ClienToServer: cannot parse =>" + BitConverter.ToString(new byte[] { byte0 }) + " " + BitConverter.ToString(new byte[] { byte1 }));
                    }
                }

                if (!canParse)
                {
                    //Console.WriteLine("[" + direction + "]" + "gameserver: " + direction);
                    Console.WriteLine(BitConverter.ToString(input));

                    System.IO.File.WriteAllBytes(
                        System.IO.Path.Combine
                        (
                            Utils.GetLogDir(), @"clientToServer\" + timeAsString + direction + ".bin"
                        ),
                        input);
                }
                else
                {
                    Console.WriteLine(BitConverter.ToString(input));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            bool hack = false;
            if (hack)
            {
                Console.WriteLine("sending hack");
                var x =  new byte[]
                {
                    /* working */
                    //0x65, 0x65, 0x13, 0x00, 0x00, 0x00, 0x6D, 0x76, 0x0C, 0xD4, 0x7E, 0x47, 0x5E, 0x61, 0xB2, 0xC5, 0xAC, 0x83, 0x96, 0x45, 0xF7, 0x2A, 0x3D, 0x9E, 0x00, 0x00, 0x00, 0x00
                    0x65, 0x65, 0x1C, 0x00, 0x00, 0x00, 0x6D, 0x76, 0x00, 0xA0, 0x2D, 0xC5, 0x00, 0x6C, 0x2C, 0xC6, 0x99, 0x30, 0x24, 0x46, 0x88, 0xC3, 0x4A, 0x86, 0x00, 0x00, 0x00, 0x00

                };
                Console.WriteLine(BitConverter.ToString(x));
                return x;
            }
            return input;
        }
    }
}
