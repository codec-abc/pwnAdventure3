using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpForwarderClient
{
    public class ServerToClientDataAnalyzer : IDataAnalyzer
    {
        interface ServerToClientAnalyzer
        {
            int GetLength(byte[] input, int offset);
            bool IsMatchingPrefix(byte[] input, int offset);
            void Parse(byte[] input, int offset);
        }

        class DummyReader : ServerToClientAnalyzer
        {
            public int GetLength(byte[] input, int offset)
            {
                return 2;
            }

            public bool IsMatchingPrefix(byte[] input, int offset)
            {

                return input[offset] == 0x00 && input[offset + 1] == 0x00;
            }

            public void Parse(byte[] input, int offset)
            {
            }
        }

        class ConnexionReader : ServerToClientAnalyzer
        {
            public int GetLength(byte[] input, int offset)
            {
                return 22;
            }

            public bool IsMatchingPrefix(byte[] input, int offset)
            {
                return input[offset] != 0x00 && input[offset + 1] == 0x00;
            }

            public void Parse(byte[] input, int offset)
            {
            }
        }

        private int _messageIndex = -1;

        private List<ServerToClientAnalyzer> _parsers =
           new List<ServerToClientAnalyzer>();

        public ServerToClientDataAnalyzer()
        {
            _parsers.Add(new DummyReader());

            System.IO.Directory.CreateDirectory(
                System.IO.Path.Combine(
                    Common.Utils.GetLogDir(),
                    @"serverToClient")
                    );
        }

        public byte[] Analyze(byte[] input, string timeAsString, string direction)
        {
            _messageIndex++;

            if (_messageIndex == 0)
            {
                //54-00-00-00-3B-7B-53-C7-56-7B-5E-C7-EE-A5-88-44-00-00-00-00-00-00

                //var x = BitConverter.ToSingle(input, 0 + 2);
                //var y = BitConverter.ToSingle(input, 4 + 2);
                //var z = BitConverter.ToSingle(input, 8 + 2);

                var returned = new byte[input.Length];

                for (int i = 0; i < input.Length; i++)
                {
                    returned[i] = input[i];
                }

                var x = BitConverter.GetBytes(-2778.0f);
                var y = BitConverter.GetBytes(-11035.0f);
                var z = BitConverter.GetBytes(10504.0f + 100.0f);

                Array.Copy(x, 0, returned, 0 + 4, 4);
                Array.Copy(y, 0, returned, 4 + 4, 4);
                Array.Copy(z, 0, returned, 8 + 4, 4);

                return returned;
            }

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
                            if (parser.IsMatchingPrefix(input, index))
                            {
                                return new int?(parser.GetLength(input, index));
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
                    //Console.WriteLine("Server To Client cannot parse " + BitConverter.ToString(new byte[] { byte0 }) + " " + BitConverter.ToString(new byte[] { byte1 }));
                    //Console.WriteLine(Utils.GetCurrentDate() + " ServerToClient: cannot parse =>" + BitConverter.ToString(input));
                }
            }

            if (!canParse)
            {

                System.IO.File.WriteAllBytes
                (
                    System.IO.Path.Combine
                    (
                        Utils.GetLogDir(), @"serverToClient\" + timeAsString + direction + ".bin"
                    ),
                    input
                );
            }

            return input;
        }
    }
}
