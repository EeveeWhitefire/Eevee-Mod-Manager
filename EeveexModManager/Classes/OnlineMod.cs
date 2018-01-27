using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Interfaces;
using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Classes
{
    public class OnlineMod : BaseMod, IOnlineMod
    {
        public string Author { get; }
        public string FullSourceUrl { get; }

        public OnlineMod(string n, bool active, bool installed, string source, 
            string modDir, GameListEnum gameId, ModCategories category, string fileId, string version, string id, string srcUrl, string author) : 
            base(n, active, installed, source, modDir, gameId, category, fileId, version, id)
        {
            Author = author;
            FullSourceUrl = srcUrl;
        }

        public Db_OnlineMod EncapsulateToDb_Online()
        {
            return new Db_OnlineMod()
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
                Id = Id,
                Author = Author,
                FullSourceUrl = FullSourceUrl
            };
        }
    }
}
