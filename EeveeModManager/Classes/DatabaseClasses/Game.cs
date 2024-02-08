using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Classes;
using EeveexModManager.Interfaces;
using LiteDB;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class Game : IGame, IGameDefault
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
        public IDictionary<string, string> KnownExecutables { get; set; }

        public GameListEnum Id { get; set; }

        public bool IsCurrent { get; set; }
        public string PluginFileExtension { get; set; }
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
        #region Constructors
        /// <summary>
        /// For the LiteDb reader
        /// </summary>
        public Game() { }

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
            string nApi, string nNxm, string nReg, GameListEnum id, IDictionary<string, string> exes, string pluginExt)
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
            PluginFileExtension = pluginExt;

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
                def.Name, def.Name_API, def.Name_Nexus, regN, def.Id, def.KnownExecutables, def.PluginFileExtension)
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
            this(iPath, dPath, exePath, def.Name, def.Name_API, def.Name_Nexus, nReg, def.Id, def.KnownExecutables, def.PluginFileExtension)
        {
            IsCurrent = isCurr;
        }
        #endregion

        public void SetDirectories(string prof, string mods, string dls, string bak)
        {
            ProfilesDirectory = prof;
            ModsDirectory = mods;
            DownloadsDirectory = dls;
            BackupsDirectory = bak;
        }
    }
}
