using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Interfaces;
using EeveexModManager.Classes;
using LiteDB;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class Db_Mod : IMod
    {
        public string Name { get; set; }
        public string ModFileName { get; set; }
        public bool Active { get; set; }
        public bool Installed { get; set; }
        public string SourceArchive { get; set; }
        public string ModDirectory { get; set; }
        public string DownloadDirectory { get; }
        public string Version { get; set; }
        public string Id { get; set; }
        public bool IsOnline { get; set; }
        public string Author { get; set; }
        public string FullSourceUri { get; set; }
        public int Priority { get; set; }

        public GameListEnum GameId { get; set; }
        public ModCategories ModCategory { get; set; }

        [BsonId]
        public string FileId { get; set; }
        
        public Mod EncapsulateToSource()
        {
            return new Mod(Name, ModFileName, Active, Installed, SourceArchive, ModDirectory, DownloadDirectory, GameId, ModCategory, 
                FileId, Priority, Version, Id, Author, FullSourceUri, IsOnline);
        }
    }
}
