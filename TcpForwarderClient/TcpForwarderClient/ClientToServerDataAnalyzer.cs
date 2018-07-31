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

        
        class JumpReader : PartReader
        {
            public int GetLength()
            {
                return 3;
            }

            public byte[] GetPrefix()
            {
                return new byte[] { 0x6A, 0x70 };
            }
        }

        class MoveReader : PartReader
        {
            public int GetLength()
            {
                return 22;
            }

            public byte[] GetPrefix()
            {
                return new byte[] { 0x6D, 0x76 };
            }
        }

        class DummyReader : PartReader
        {
            public int GetLength()
            {
                return 2;
            }

            public byte[] GetPrefix()
            {
                return new byte[] { 0x00, 0x00 };
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

        class PickupReader : PartReader
        {
            public int GetLength()
            {
                return 6;
            }

            public byte[] GetPrefix()
            {
                return new byte[] { 0x65, 0x65 };
            }
        }

        public ClientToServerDataAnalyzer()
        {
            _parsers.Add(new JumpReader());
            _parsers.Add(new MoveReader());
            _parsers.Add(new DummyReader());
            //_parsers.Add(new LeftClickReader());
            _parsers.Add(new PickupReader());

            System.IO.Directory.CreateDirectory(
                System.IO.Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            @"retroIngeneering\clientToServer\"));
        }

        private List<PartReader> _parsers =
            new List<PartReader>();

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
                                    return new int?(parser.GetLength());
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        )
                        .Where(l => l.HasValue).ToList();

                    if (offsets.Any())
                    {
                        index += offsets.First().Value;
                    }
                    else
                    {
                        canParse = false;
                        Console.WriteLine("cannot parse " + BitConverter.ToString(new byte[] { byte0 }) + " " + BitConverter.ToString(new byte[] { byte1 }));
                    }
                }

                if (!canParse)
                {
                    var truc = BitConverter.ToString(input);
                    Console.WriteLine("[" + direction + "]" + "gameserver: " + direction);
                    Console.WriteLine(truc);

                    System.IO.File.WriteAllBytes(
                        System.IO.Path.Combine
                        (
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            @"retroIngeneering\clientToServer\" + timeAsString + direction + ".bin"
                        ),
                        input);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            return input;
        }
    }
}
