using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;

using EeveexModManager.Interfaces;
using System.Threading;
using System.IO;
using System.Windows;
using System.Security.AccessControl;
using System.Security.Principal;

namespace EeveexModManager.Classes
{
    public class NamedPipeManager : INamedPipe
    {
        public string Name { get; }

        private NamedPipeServerStream NamedPipeStream_Server;
        private NamedPipeClientStream NamedPipeStream_Client;

        private Thread Th_namedPipeListening;
        private Thread Th_namedPipeWriting;
        private App MainApp;

        private Action<string> MessageReceivedHandler;

        public bool IsRunning = false;

        public NamedPipeManager(string n, bool isClient, App main)
        {
            Name = n;
            MainApp = main;
            if (isClient)
            {
                NamedPipeStream_Client = new NamedPipeClientStream(".", Name, PipeDirection.InOut, PipeOptions.Asynchronous);
                Th_namedPipeWriting = new Thread(Send_NamedPipe);
            }
            else
            {
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                PipeAccessRule psRule = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, AccessControlType.Allow);
                PipeSecurity pipeSecurity = new PipeSecurity();
                pipeSecurity.AddAccessRule(psRule);
                
                NamedPipeStream_Server = new NamedPipeServerStream(Name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512, pipeSecurity);
                Th_namedPipeListening = new Thread(Listen_NamedPipe);
            }
        }

        public void CloseConnections()
        {
            if(Th_namedPipeWriting != null)
            {
                Th_namedPipeWriting.Abort();
                
                NamedPipeStream_Client.Close();
            }
            else
            {
                Th_namedPipeListening.Abort();

                NamedPipeStream_Server.Close();
            }

        }

        public void InitServer()
        {
            Th_namedPipeListening.Start();
            IsRunning = true;
        }

        public void ChangeMessageReceivedHandler(Action<string> handler)
        {
            MessageReceivedHandler = handler;
        }

        public void SendToServer(object message)
        {
            try
            {
                Th_namedPipeWriting.Start(message);
                Th_namedPipeWriting.Join();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            MainApp.ExitEVX();
        }

        void Listen_NamedPipe()
        {
            while (true)
            {
                try
                {
                    NamedPipeStream_Server.WaitForConnection();
                    PipeStreamString_In streamStr = new PipeStreamString_In(NamedPipeStream_Server);
                    MessageReceivedHandler(streamStr.ReadString());

                }
                catch (Exception)
                {
                }

                Thread.Sleep(2000);
            }
        }

        void Send_NamedPipe(object data)
        {
            try
            {
                NamedPipeStream_Client.Connect();

                PipeStreamString_Out streamStr = new PipeStreamString_Out(NamedPipeStream_Client);

                streamStr.WriteString(data.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            MainApp.ExitEVX();
        }
    }
}
