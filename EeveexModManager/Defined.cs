using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager
{
    public class Defined
    {
        public const int MODPICKINGBUTTON_SIZE = 40;
        public const string DEFAULTMODVERSION = "1.0.0";
        public const string DEFAULTMODID = "Unknown";
        public const string DEFAULTMODAUTHOR = "Unknown";
        public const string DEFAULTSOURCEURI = "Unknown";
        public const string NEXUSAPI_BASE = @"https://api.nexusmods.com/v1";
    }

    public class Assistant
    {

        public static List<string> GetAllFilesInDir(string d)
        {
            List<string> files = new List<string>();

            ProcessDirectory(ref files, d);

            return files;
        }
        public static void ProcessDirectory(ref List<string> files, string p)
        {
            files.AddRange(Directory.GetFiles(p));

            var dirs = Directory.GetDirectories(p);
            foreach (var item in dirs)
            {
                ProcessDirectory(ref files, item);
            }
        }
    }
}
