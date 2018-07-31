using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPForwarder
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    namespace BrunoGarcia.Net
    {
        public class TcpForwarderSlim
        {
            private readonly Socket _mainSocket = 
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            public static void Main(string[] args)
            {

                var ipAddress = "10.208.162.96";
                var ipAddress2 = "10.208.162.110";
                var port_in = 3333;
                var port_out = 3333;

                var tcpForwarder = new TcpForwarderSlim();

                tcpForwarder.Start(
                   new IPEndPoint(IPAddress.Parse(ipAddress), port_in),
                   new IPEndPoint(IPAddress.Parse(ipAddress2), port_out));
            }

            public void Start(IPEndPoint local, IPEndPoint remote)
            {
                _mainSocket.Bind(local);
                _mainSocket.Listen(100000);

                while (true)
                {
                    var source = _mainSocket.Accept();
                    var destination = new TcpForwarderSlim();
                    var state = new State(source, destination._mainSocket);
                    destination.Connect(remote, source);
                    source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }

            private void Connect(EndPoint remoteEndpoint, Socket destination)
            {
                var state = new State(_mainSocket, destination);
                _mainSocket.Connect(remoteEndpoint);
                _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
            }

            private static void OnDataReceive(IAsyncResult result)
            {
                var state = (State)result.AsyncState;
                try
                {
                    var bytesRead = state.SourceSocket.EndReceive(result);
                    if (bytesRead > 0)
                    {
                        var bytes = new byte[bytesRead];
                        for (int i = 0; i < bytesRead; i++)
                        {
                            bytes[i] = state.Buffer[i];
                        }

                        var now = DateTime.UtcNow;

                        var data = BitConverter.ToString(bytes);

                        Console.WriteLine("state " + state.SourceSocket.RemoteEndPoint + " to " + state.DestinationSocket.RemoteEndPoint);
                        Console.WriteLine(data);

                        state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                        state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                    }
                }
                catch
                {
                    state.DestinationSocket.Close();
                    state.SourceSocket.Close();
                }
            }

            private class State
            {
                public Socket SourceSocket { get; private set; }
                public Socket DestinationSocket { get; private set; }
                public byte[] Buffer { get; private set; }

                public State(Socket source, Socket destination)
                {
                    SourceSocket = source;
                    DestinationSocket = destination;
                    Buffer = new byte[4096];
                }
            }
        }
    }
}
