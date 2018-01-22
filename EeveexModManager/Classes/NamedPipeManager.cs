using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;

using EeveexModManager.Interfaces;
using System.Threading;
using System.IO;

namespace EeveexModManager.Classes
{
    public class NamedPipeManager : INamedPipe
    {
        public string Name { get; }

        private NamedPipeServerStream NamedPipeStream_Server;
        private NamedPipeClientStream NamedPipeStream_Client;

        private Thread Th_namedPipeListening;
        private Thread Th_namedPipeWriting;

        public NamedPipeManager(string n)
        {
            Name = n;
            NamedPipeStream_Server = new NamedPipeServerStream(Name, PipeDirection.In, 1);
            NamedPipeStream_Client = new NamedPipeClientStream(".", Name, PipeDirection.Out);

            Th_namedPipeWriting = new Thread(Send_NamedPipe);
            Th_namedPipeListening = new Thread(Listen_NamedPipe);

        }

        public void CloseConnections()
        {
            if (Th_namedPipeWriting.ThreadState == ThreadState.Running)
            {
                Th_namedPipeWriting.Abort();
            }

            if (Th_namedPipeListening.ThreadState == ThreadState.Running)
            {
                Th_namedPipeListening.Abort();
            }

            NamedPipeStream_Server.Close();
            NamedPipeStream_Client.Close();

        }

        public void InitServer(Action<string> handler)
        {
            Th_namedPipeListening.Start(handler);
        }

        public void SendToServer(object message)
        {
            Th_namedPipeWriting.Start(message);
            Th_namedPipeWriting.Join();

        }

        void Listen_NamedPipe(object handler)
        {
            while (true)
            {
                NamedPipeStream_Server.WaitForConnection();

                PipeStreamString_In streamStr = new PipeStreamString_In(NamedPipeStream_Server);
                ((Action<string>)(handler))(streamStr.ReadString());
            }
        }

        void Send_NamedPipe(object data)
        {
            NamedPipeStream_Client.Connect();

            PipeStreamString_Out streamStr = new PipeStreamString_Out(NamedPipeStream_Client);

            streamStr.WriteString((string)data, ref NamedPipeStream_Client);
        }
    }
}
