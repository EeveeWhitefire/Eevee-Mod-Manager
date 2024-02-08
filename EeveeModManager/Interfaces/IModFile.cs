using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Interfaces
{
    public enum ModFileExtensions
    {
        ASSETS_ARCHIVE,
        PLUGIN,
        TEXTURE,
        MESH,
        OTHER
    }

    interface IModFile
    {
        string RelativePath { get; }
        string FullPath { get;}
        string Name { get;}
        Mod AssociatedMod { get;}
        string Directory { get;}
        string RelativeDirectory { get;}
        ModFileExtensions Extension { get; }
    }
}
