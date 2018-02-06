using System.IO;
using System.Text;

namespace EeveexModManager.Classes
{
    // Defines the data protocol for reading and writing strings on our stream
    public class PipeStreamString_In
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public PipeStreamString_In(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;
            try
            {
                len = ioStream.ReadByte() * 256;
                len += ioStream.ReadByte();
                byte[] inBuffer = new byte[len];
                ioStream.Read(inBuffer, 0, len);
                return streamEncoding.GetString(inBuffer);
            }
            catch (System.Exception)
            {
                return string.Empty;
            }

        }
    }
}
