using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Classes
{
    public class ModPlugin : ModFile
    {
        public ModPlugin(Mod m, FileInfo info) : base(m, info, Interfaces.ModFileExtensions.PLUGIN)
        {
        }
    }
}
