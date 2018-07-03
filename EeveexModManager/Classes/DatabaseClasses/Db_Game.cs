using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Classes;
using EeveexModManager.Interfaces;
using LiteDB;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class Db_Game : IGame, IGameDefault
    {
        #region Fields
        public string DataPath { get; set; }
        public string InstallationPath { get; set; }
        public string ExecutablePath { get; set; }
        public string Name_Nexus { get; set; }
        public string ExecutableName { get; } = null;

        [BsonId]
        public string Name { get; set; }
        public string Name_API { get; set; }
        public string Name_Registry { get; set; }
        public string RelativeDataPath { get; set; }
        public string[] Registry_Names { get; protected set; } = null;
        public string ModsDirectory { get; set; }
        public string DownloadsDirectory { get; set; }
        public string ProfilesDirectory { get; set; }
        public string BackupsDirectory { get; set; }
        public Dictionary<string, string> KnownExecutables { get; set; }

        public GameListEnum Id { get; set; }

        public bool IsCurrent { get; set; }
        #endregion

        public Game EncapsulateToSource()
            => new Game(InstallationPath, DataPath, ExecutablePath, 
                this, Name_Registry, IsCurrent);
    }
}
