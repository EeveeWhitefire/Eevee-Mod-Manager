using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Windows;

namespace EeveexModManager.Classes
{
    // Defines the data protocol for reading and writing strings on our stream
    public class PipeStreamString_Out
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public PipeStreamString_Out(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public bool WriteString(string outString)
        {
            try
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
            
        }
    }
}
