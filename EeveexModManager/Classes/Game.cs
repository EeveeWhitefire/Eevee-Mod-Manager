using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Interfaces;
namespace EeveexModManager.Classes
{
    public class Game : IGame, IGameDefault
    {
        #region Properties
        public string DataPath { get; }
        public string InstallationPath { get; }
        public string ExecutablePath { get; }

        public string Name { get; }
        public string Name_Nexus { get; protected set; }
        public string Name_API { get; }
        public string RelativeDataPath { get; }
        public string Name_Registry { get; protected set; }
        public string[] Registry_Names { get; protected set; } = null;
        public string ModsDirectory { get; protected set; }
        public string DownloadsDirectory { get; protected set; }
        public string ProfilesDirectory { get; protected set; }
        public string BackupsDirectory { get; protected set; }
        public string ExecutableName { get; } = null;
        public Dictionary<string, string> KnownExecutables { get; }

        public GameListEnum Id { get; }
        public bool IsCurrent { get; protected set; } = false;
        #endregion
        #region Static Methods

        static string GetExecutablePath(string n, string path)
        {
            return ProcessDirectory(path, n);
        }
        static string ProcessDirectory(string p, string n)
        {
            foreach (var item in Directory.GetFiles(p))
            {
                if (item.EndsWith(n))
                {
                    return item;
                }
            }

            return string.Empty;
        }

        public List<GameApplication> AutoDetectApplications()
        {
            List<GameApplication> apps = new List<GameApplication>();

            var inf = Directory.EnumerateFiles(InstallationPath, "*.*", SearchOption.AllDirectories)
                .Select(x => new KeyValuePair<string, string>(new FileInfo(x).Name, x));

            foreach (var file in inf.Where(s => KnownExecutables.Values.Contains(s.Key)))
            {
                apps.Add(new GameApplication(KnownExecutables.FirstOrDefault(y => y.Value == file.Key).Key, file.Value, Id));
            }

            return apps;
        }
        #endregion
        /// <summary>
        /// Creates a Game object by its name (of the game)
        /// </summary>
        /// <param name="n">The name of the game</param>
        /// <param name="iPath">Where the game is installed</param>
        /// <param name="regN">The name of the game in the registry</param>
        /// <returns></returns>
        public static Game CreateByName(string iPath, string regN, IGameDefault def)
            => new Game(iPath, def, regN);

        public void ToggleIsCurrentGame()
        {
            IsCurrent = !IsCurrent;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iPath">Installation path</param>
        /// <param name="dPath">Game data path (to which you install mods)</param>
        /// <param name="exePath">Executable path</param>
        /// <param name="n">Full name of the game</param>
        /// <param name="nApi">Name of the game in Nexus Mods' API</param>
        /// <param name="nReg">Display name of the game in the registry (under Uninstall)</param>
        /// <param name="id">Nexus Mods' generated ID for the game</param>
        public Game(string iPath, string dPath, string ePath, string n, 
            string nApi, string nNxm, string nReg, GameListEnum id, Dictionary<string,string> exes)
        {
            Name = n;
            InstallationPath = iPath;
            DataPath = dPath;
            ExecutablePath = ePath;
            Name_API = nApi;
            Name_Nexus = nNxm;
            Id = id;
            Name_Registry = nReg;
            KnownExecutables = exes;

            string idAsString = Id.ToString();

            ModsDirectory = InstallationPath[0] + ":\\EMM\\Mods\\" + idAsString;
            DownloadsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EMM\\Downloads\\" + idAsString;
            ProfilesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EMM\\Profiles\\" + idAsString;
            BackupsDirectory = InstallationPath[0] + ":\\EMM\\Backups\\" + idAsString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iPath">Installation Path</param>
        /// <param name="def">Default settings of the game</param>
        /// <param name="regN">Registry name of the game</param>
        public Game(string iPath, IGameDefault def, string regN) : 
            this(iPath, $"{iPath}{def.RelativeDataPath}", GetExecutablePath(def.ExecutableName, iPath), 
                def.Name, def.Name_API, def.Name_Nexus, regN, def.Id, def.KnownExecutables)
        { }
        /// <summary>
        /// Full Constructor
        /// </summary>
        /// <param name="iPath">Installation path</param>
        /// <param name="dPath">Game data path (to which you install mods)</param>
        /// <param name="exePath">Executable path</param>
        /// <param name="n">Full name of the game</param>
        /// <param name="nApi">Name of the game in Nexus Mods' API</param>
        /// <param name="nReg">Display name of the game in the registry (under Uninstall)</param>
        /// <param name="id">Nexus Mods' generated ID for the game</param>
        /// <param name="isCurr">Whether this game is the current one</param>
        public Game(string iPath, string dPath, string exePath, 
            IGameDefault def, string nReg, bool isCurr) : 
            this(iPath, dPath, exePath, def.Name, def.Name_API, def.Name_Nexus, nReg, def.Id, def.KnownExecutables)
        {
            IsCurrent = isCurr;
        }

        public void SetDirectories(string prof, string mods, string dls, string bak)
        {
            ProfilesDirectory = prof;
            ModsDirectory = mods;
            DownloadsDirectory = dls;
            BackupsDirectory = bak;
        }

        public virtual Db_Game EncapsulateToDb()
        {
            return new Db_Game()
            {
                DataPath = DataPath,
                ExecutablePath = ExecutablePath,
                Id = Id,
                InstallationPath = InstallationPath,
                IsCurrent = IsCurrent,
                Name = Name,
                Name_Nexus = Name_Nexus,
                Name_API = Name_API,
                Name_Registry = Name_Registry,
                ModsDirectory = ModsDirectory,
                DownloadsDirectory = DownloadsDirectory,
                ProfilesDirectory = ProfilesDirectory,
                BackupsDirectory = BackupsDirectory
            };
        }
    }
}
