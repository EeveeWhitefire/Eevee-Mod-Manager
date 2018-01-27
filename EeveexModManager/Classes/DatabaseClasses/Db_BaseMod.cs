using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Interfaces;
using EeveexModManager.Classes;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class Db_BaseMod : IMod
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Installed { get; set; }
        public string SourceArchive { get; set; }
        public string ModDirectory { get; set; }
        public string Version { get; set; }
        public string Id { get; set; }
        public string FileId { get; set; }

        public GameListEnum GameId { get; set; }
        public ModCategories ModCategory { get; set; }

        public Db_BaseMod()
        { }

        public BaseMod EncapsulateToSource()
        {
            return new BaseMod(Name, Active, Installed, SourceArchive, ModDirectory, GameId, ModCategory, Version, Id);
        }
    }
}
