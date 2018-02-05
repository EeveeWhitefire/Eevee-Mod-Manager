using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Interfaces;
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Define;

namespace EeveexModManager.Classes
{
    public class Mod : IMod
    {
        public string Name { get;}
        public string ModFileName { get; }
        public bool Active { get; protected set; }
        public bool Installed { get;}
        public string SourceArchive { get; }
        public string ModDirectory { get;}
        public string DownloadDirectory { get; }
        public string Version { get; }
        public string Id { get; }
        public bool IsOnline { get; protected set; } = false;
        public string Author { get; protected set; } = "Unknown";
        public string FullSourceUri { get; protected set; } = "Unknown";

        public GameListEnum GameId { get;}
        public ModCategories ModCategory { get;}

        public string FileId { get; set; }

        public Mod(string n, string fileN, bool active, bool installed, string source, string modDir, string dlDir, GameListEnum gameId, ModCategories category, string fileId, 
            string version = Defined.DEFAULTMODVERSION, string id = Defined.DEFAULTMODID, string author = Defined.DEFAULTMODAUTHOR, string srcUri = Defined.DEFAULTSOURCEURI,
            bool isOn = false)
        {
            Name = n;
            IsOnline = isOn;
            ModFileName = fileN;
            Active = active;
            Installed = installed;
            SourceArchive = source;
            ModDirectory = modDir;
            DownloadDirectory = dlDir;
            GameId = gameId;
            ModCategory = category;
            FileId = fileId;
            Version = version;
            Id = id;
            Author = author;
            FullSourceUri = srcUri;
        }

        public void ToggleIsActive()
        {
            Active = !Active;
        }

        public Db_Mod EncapsulateToDb()
        {
            return new Db_Mod()
            {
                Name = Name,
                Active = Active,
                Installed = Installed,
                SourceArchive = SourceArchive,
                ModDirectory = ModDirectory,
                GameId = GameId,
                ModCategory = ModCategory,
                FileId = FileId,
                Version = Version,
                IsOnline = IsOnline,
                ModFileName = ModFileName,
                Id = Id,
                Author = Author,
                FullSourceUri = FullSourceUri
            };
        }
    }
}
