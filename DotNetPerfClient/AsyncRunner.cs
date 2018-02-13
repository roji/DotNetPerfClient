using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetPerfClient
{
    class AsyncRunner
    {
        readonly AwaitableSocket _awaitableWriteSocket, _awaitableReadSocket;
        readonly Socket _socket;
        readonly int _outgoingLen, _incomingLen;
        readonly byte[] _outgoing, _incoming;

        const int Port = 8888;

        public AsyncRunner(string hostname, int outgoingLen, int incomingLen)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(Dns.GetHostAddresses(hostname)[0], Port);
            _socket.NoDelay = true;
            _outgoingLen = outgoingLen;
            _incomingLen = incomingLen;
            _outgoing = new byte[outgoingLen];
            _incoming = new byte[incomingLen];
            _awaitableWriteSocket = new AwaitableSocket(new SocketAsyncEventArgs(), _socket);
            _awaitableWriteSocket.SetBuffer(_outgoing, 0, outgoingLen);
            _awaitableReadSocket = new AwaitableSocket(new SocketAsyncEventArgs(), _socket);
            _awaitableReadSocket.SetBuffer(_incoming, 0, incomingLen);
        }

        internal async Task Run()
        {
            try
            {
                while (true)
                {
                    await _awaitableWriteSocket.SendAsync();
                    var toRead = _incomingLen;
                    while (toRead > 0)
                    {
                        await _awaitableReadSocket.ReceiveAsync();
                        var read = _awaitableReadSocket.BytesTransferred;
                        if (read == 0)
                            return;
                        toRead -= read;
                    }

                    Interlocked.Increment(ref Program.Counter);
                    if (Program.IsRunning == 0)
                        return;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Exception: " + e);
            }
        }
    }
}
