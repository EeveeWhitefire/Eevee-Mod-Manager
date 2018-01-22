using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Interfaces;

namespace EeveexModManager.Classes
{
    public class BaseMod : IMod
    {
        public string Name { get;}
        public bool Active { get; protected set; }
        public bool Installed { get;}
        public string SourceArchive { get; }
        public string ModDirectory { get;}
        public string Version { get; }
        public string Id { get; }

        public GameListEnum GameId { get;}
        public ModCategories ModCategory { get;}
        [Key]
        public string FileId { get; set; }

        public BaseMod(string n, bool active, bool installed, string source, 
            string modDir, GameListEnum gameId, ModCategories category, string fileId, string version = "1.0.0", string id = "Unknown")
        {
            Name = n;
            Active = active;
            Installed = installed;
            SourceArchive = source;
            ModDirectory = modDir;
            GameId = gameId;
            ModCategory = category;
            FileId = fileId;
            Version = version;
            Id = id;
        }

        public void Set_isActive(bool to)
        {
            if(Active != to)
            {
                Active = to;
            }
        }

    }
}
