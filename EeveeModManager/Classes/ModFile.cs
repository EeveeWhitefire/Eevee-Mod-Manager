using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Interfaces;

namespace EeveexModManager.Classes
{
    public class ModFile : IModFile
    {
        public string RelativePath { get; protected set; }
        public string FullPath { get; protected set; }
        public string Name { get; protected set; }
        public Mod AssociatedMod { get; protected set; }
        public string Directory { get; protected set; }
        public string RelativeDirectory { get; protected set; }
        public ModFileExtensions Extension { get; protected set; }

        public ModFile(Mod m, FileInfo info, ModFileExtensions ext)
        {
            Extension = ext;
            FullPath = info.FullName;
            AssociatedMod = m;
            RelativePath = FullPath.Replace(AssociatedMod.ModDirectory + "\\", string.Empty);
            Directory = info.Directory.FullName;
            Name = info.Name + info.Extension;
            RelativeDirectory = Directory.Replace(AssociatedMod.ModDirectory + "\\", string.Empty);
        }
    }
}
