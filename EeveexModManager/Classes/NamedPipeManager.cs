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
        private PipeSecurity pipeSecurity;

        private Action<string> MessageReceivedHandler;

        public bool IsRunning = false;

        public NamedPipeManager(string n)
        {
            Name = n;
            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            PipeAccessRule psRule = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, AccessControlType.Allow);
            pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(psRule);

            NamedPipeStream_Server = new NamedPipeServerStream(Name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 512, 512, pipeSecurity);
        }

        public void CloseConnections()
        {
            IsRunning = false;
            try
            {
                NamedPipeStream_Server.Dispose();
                NamedPipeStream_Server.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }
        public void ChangeMessageReceivedHandler(Action<string> handler)
        {
            MessageReceivedHandler = handler;
        }

        public void Listen_NamedPipe(Dispatcher d)
        {
            IsRunning = true;

            try
            {
                NamedPipeStream_Server.WaitForConnection();
                if (NamedPipeStream_Server.IsConnected)
                {

                    Task.Run(() =>
                   d.Invoke(() =>
                   {
                       PipeStreamString_In streamStr = new PipeStreamString_In(NamedPipeStream_Server);
                       MessageReceivedHandler(streamStr.ReadString());
                       NamedPipeStream_Server.Disconnect();
                   }, DispatcherPriority.Background));
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
