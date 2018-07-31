using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpForwarderClient
{
    public class ServerToClientDataAnalyzer : IDataAnalyzer
    {
        //class MoveReader : PartReader
        //{
        //    public int GetLength()
        //    {
        //        return 24;
        //    }

        //    public byte[] GetPrefix()
        //    {
        //        return new byte[] { 0x6D, 0x76 };
        //    }
        //}

        private List<PartReader> _parsers =
           new List<PartReader>();

        public ServerToClientDataAnalyzer()
        {
            //_parsers.Add(new MoveReader());

            System.IO.Directory.CreateDirectory(
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    @"retroIngeneering\serverToClient\")
                    );
        }

        public byte[] Analyze(byte[] input, string timeAsString, string direction)
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
                    //Console.WriteLine("cannot parse " + BitConverter.ToString(new byte[] { byte0 }) + " " + BitConverter.ToString(new byte[] { byte1 }));
                }
            }

            if (!canParse)
            {

                System.IO.File.WriteAllBytes
                (
                    System.IO.Path.Combine
                    (
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        @"retroIngeneering\serverToClient\" + timeAsString + direction + ".bin"
                    ),
                    input
                );
            }

            return input;
        }
    }
}
