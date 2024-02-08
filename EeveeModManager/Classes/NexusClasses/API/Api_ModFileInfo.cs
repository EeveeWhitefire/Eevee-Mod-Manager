using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Classes.NexusClasses.API
{
    public class Api_ModFileInfo
    {
        public ulong file_id { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public int category_id { get; set; }
        public string category_name { get; set; }
        public bool is_primary { get; set; }
        public string size { get; set; }
        public string file_name { get; set; }
        public string mod_version { get; set; }
    }
}
