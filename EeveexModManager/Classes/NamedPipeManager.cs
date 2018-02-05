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
using System.Windows.Threading;

namespace EeveexModManager.Classes
{
    public class NamedPipeManager : INamedPipe
    {
        public string Name { get; }

        private NamedPipeServerStream NamedPipeStream_Server;
        private NamedPipeClientStream NamedPipeStream_Client;

        private Action<string> MessageReceivedHandler;

        public bool IsRunning = false;

        public NamedPipeManager(string n, bool isServer)
        {
            Name = n;
            if (!isServer)
            {
                NamedPipeStream_Client = new NamedPipeClientStream(".", Name, PipeDirection.InOut, PipeOptions.Asynchronous);
            }
            else
            {
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                PipeAccessRule psRule = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, AccessControlType.Allow);
                PipeSecurity pipeSecurity = new PipeSecurity();
                pipeSecurity.AddAccessRule(psRule);
                
                NamedPipeStream_Server = new NamedPipeServerStream(Name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512, pipeSecurity);
            }
        }

        public void CloseConnections()
        {
            try
            {
                NamedPipeStream_Client.Close();
            }
            catch (Exception)
            {
                try
                {
                    NamedPipeStream_Server.Close();
                }
                catch (Exception)
                {
                }
            }

        }
        public void ChangeMessageReceivedHandler(Action<string> handler)
        {
            MessageReceivedHandler = handler;
        }
        

        public void Send_NamedPipe(object data)
        {
            try
            {
                NamedPipeStream_Client.Connect(2000);

                PipeStreamString_Out streamStr = new PipeStreamString_Out(NamedPipeStream_Client);

                streamStr.WriteString(data.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void Listen_NamedPipe(Dispatcher d)
        {
            IsRunning = true;

            while (true)
            {
                NamedPipeStream_Server.WaitForConnection();
                d.Invoke(() =>
                {
                    PipeStreamString_In streamStr = new PipeStreamString_In(NamedPipeStream_Server);
                    MessageReceivedHandler(streamStr.ReadString());
                });
            }
        }

    }
}
