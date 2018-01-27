using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Classes.JsonClasses.API
{
    public class Api_ModFileInfo
    {
        public ulong id { get; set; }
        public int game_id { get; set; }
        public ulong owner_id { get; set; }
        public int category_id { get; set; }
        public bool primary { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string uri { get; set; }
        public string size { get; set; }
        public string version { get; set; }
    }
}
