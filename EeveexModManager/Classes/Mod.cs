using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Classes
{
    public class Mod
    {
        public ulong Id { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Installed { get; set; }
    }
}
