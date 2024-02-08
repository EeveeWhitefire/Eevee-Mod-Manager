using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Threading;
using System.IO;
using System.Windows;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Threading;

using EeveexModManager.Interfaces;

namespace EeveexModManager.Classes
{
    public class NamedPipeManager : INamedPipe
    {
        public string Name { get; } = Defined.NAMED_PIPE_NAME;

        private NamedPipeServerStream NamedPipeStream_Server;
        private PipeSecurity pipeSecurity;

        private Action<string> MessageReceivedHandler;

        public bool IsRunning = false;
        private const int MAX_MESSAGE_LENGTH = 512;

        public NamedPipeManager()
        {
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

        public async Task Listen_NamedPipe(Dispatcher d)
        {
            if(!IsRunning) IsRunning = true;
            bool connectedOrWaiting = false;

            byte[] buffer = new byte[MAX_MESSAGE_LENGTH];

            if (!connectedOrWaiting)
            {
                NamedPipeStream_Server.BeginWaitForConnection(async (a) =>
                {
                    if (IsRunning)
                    {
                        NamedPipeStream_Server.EndWaitForConnection(a);
                        await Task.Run(async () => await Listen_NamedPipe(d));
                        connectedOrWaiting = true;

                        if (NamedPipeStream_Server.IsConnected)
                        {
                            int inCount = NamedPipeStream_Server.Read(buffer, 0, MAX_MESSAGE_LENGTH);
                            NamedPipeStream_Server.Disconnect();
                            if (inCount > 0)
                            {
                                string input = Encoding.UTF8.GetString(buffer, 0, inCount);
                                await d.BeginInvoke(MessageReceivedHandler, DispatcherPriority.Background, input);
                            }

                            connectedOrWaiting = false;
                            await Task.CompletedTask;
                        }
                    }
                }, null);
            }
            await Task.CompletedTask;
        }
    }
}
