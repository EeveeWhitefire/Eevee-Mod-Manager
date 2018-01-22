using EeveexModManager.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Classes
{
    public class OnlineMod : BaseMod, IOnlineMod, IMod
    {
        public string Author { get; }
        public string FullSourceUrl { get; }

        public OnlineMod(string n, bool active, bool installed, string source, 
            string modDir, GameListEnum gameId, ModCategories category, string fileId, string version, string id) : 
            base(n, active, installed, source, modDir, gameId, category, fileId, version, id)
        {
        }
    }
}
