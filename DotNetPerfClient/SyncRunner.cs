using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DotNetPerfClient
{
    class SyncRunner
    {
        readonly Thread _thread;
        readonly Socket _socket;
        readonly int _outgoingLen, _incomingLen;
        readonly byte[] _outgoing, _incoming;

        const int Port = 8888;

        public SyncRunner(string hostname, int outgoingLen, int incomingLen)
        {
            _thread = new Thread(Run);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(Dns.GetHostAddresses(hostname)[0], Port);
            _socket.NoDelay = true;
            _outgoingLen = outgoingLen;
            _incomingLen = incomingLen;
            _outgoing = new byte[outgoingLen];
            _incoming = new byte[incomingLen];
        }

        public void Start() => _thread.Start();
        public void Join() => _thread.Join();

        void Run()
        {
            try
            {
                while (true)
                {
                    _socket.Send(_outgoing);
                    var toRead = _incomingLen;
                    while (toRead > 0)
                    {
                        var read = _socket.Receive(_incoming);
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
