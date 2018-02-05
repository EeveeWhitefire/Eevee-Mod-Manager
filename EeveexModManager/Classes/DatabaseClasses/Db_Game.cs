using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Classes;
using EeveexModManager.Interfaces;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class Db_Game : IGame
    {
        #region Fields
        public string DataPath { get; set; }
        public string InstallationPath { get; set; }
        public string ExecutablePath { get; set; }
        public string Name_Nexus { get; set; }

        public string Name { get; set; }
        public string Name_API { get; set; }
        public string Name_Registry { get; set; }
        public string ModsDirectory { get; set; }
        public string DownloadsDirectory { get; set; }

        public GameListEnum Id { get; set; }

        public bool IsCurrent { get; set; }
        #endregion

        public Db_Game()
        { }

        public Game EncapsulateToSource()
        {
            return new Game(InstallationPath, DataPath, ExecutablePath, Name,Name_Nexus,  Name_API, Name_Registry, Id, IsCurrent);
        }
    }
}
