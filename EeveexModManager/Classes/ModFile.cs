using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Classes
{
    public class ModFile
    {
        public string RelativePath { get; protected set; }
        public string FullPath { get; protected set; }
        public string Name { get; protected set; }
        public Mod AssociatedMod { get; protected set; }
        public string Directory { get; protected set; }
        public string RelativeDirectory { get; protected set; }

        public ModFile(Mod m, string fullPath)
        {
            FileInfo info = new FileInfo(fullPath);

            FullPath = fullPath;
            AssociatedMod = m;
            RelativePath = FullPath.Replace(AssociatedMod.ModDirectory + "\\", string.Empty);
            Directory = info.Directory.FullName;
            Name = info.Name + info.Extension;
            RelativeDirectory = Directory.Replace(AssociatedMod.ModDirectory + "\\", string.Empty);
        }
    }
}
