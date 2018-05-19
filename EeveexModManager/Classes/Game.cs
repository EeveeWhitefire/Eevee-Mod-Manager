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
        public string Name_Registry { get; protected set; }
        public string[] Registry_Names { get; protected set; } = null;
        public string ModsDirectory { get; protected set; }
        public string DownloadsDirectory { get; protected set; }
        public string ProfilesDirectory { get; protected set; }
        public string BackupsDirectory { get; protected set; }
        public string ExecutableName { get; } = null;

        public GameListEnum Id { get; }
        public bool IsCurrent { get; protected set; } = false;
        #endregion
        #region Static Methods

        static string GetExecutablePath(string n, string path)
        {
            return ProcessDirectory(path, n);
        }
        public static string GetDataPath(GameListEnum id, string path)
        {
            switch (id)
            {
                case GameListEnum.SkyrimSE:
                    return path + @"\Data";
                case GameListEnum.Skyrim:
                    return path + @"\Data";
                case GameListEnum.Oblivion:
                    return path + @"\Data";
                case GameListEnum.Witcher3:
                    return path + @"\Data";
                case GameListEnum.FalloutNV:
                    return path + @"\Data";
                case GameListEnum.Fallout4:
                    return path + @"\Data";
                case GameListEnum.Fallout3:
                    return path + @"\Data";
                case GameListEnum.DragonAge2:
                    return path + @"\Data";
                case GameListEnum.DragonAgeOrigins:
                    return path + @"\Data";
                default:
                    return path + @"\Data";
            }
        }
        static string ProcessDirectory(string p, string n)
        {
            bool found = false;
            foreach (var item in Directory.GetFiles(p))
            {
                if (item.EndsWith(n))
                {
                    return item;
                }
            }

            if (!found)
            {
                var dirs = Directory.GetDirectories(p);
                foreach (var item in dirs)
                {
                    return ProcessDirectory(p, n);
                }
                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        public List<GameApplication> AutoDetectApplications()
        {
            List<GameApplication> apps = new List<GameApplication>();
            List<string> names = new List<string>();
            IEnumerable<KeyValuePair<string, string>> exes;

            switch (Name)
            {
                case "TESV : Skyrim Special Edition":
                    names = new List<string>()
                    {
                        "SKSE.exe", "SkyrimSE.exe", "SkyrimSELauncher.exe"
                    };
                    break;
                case "TESV : Skyrim":
                    break;
                case "Fallout : New Vegas":
                    names = new List<string>()
                    {
                        "FalloutNV.exe", "FalloutNVLauncher.exe"
                    };
                    break;
                case "Fallout 4":
                    break;
                case "Fallout 3":
                    break;
                case "Dragon Age II":
                    break;
                default:
                    break;
            }

            exes = Directory.EnumerateFiles(InstallationPath, "*.*", SearchOption.AllDirectories)
                .Where(s => names.Any(s.Contains)).Select(x => new KeyValuePair<string, string>(x.Split('\\').Last(), x));

            foreach (var item in exes)
            {
                apps.Add(new GameApplication(item.Key, item.Value, Id));
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
            string nApi, string nNxm, string nReg, GameListEnum id)
        {
            Name = n;
            InstallationPath = iPath;
            DataPath = dPath;
            ExecutablePath = ePath;
            Name_API = nApi;
            Name_Nexus = nNxm;
            Id = id;
            Name_Registry = nReg;

            string idAsString = Id.ToString();

            ModsDirectory = InstallationPath[0] + ":\\EVX\\Mods\\" + idAsString;
            DownloadsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EVX\\Downloads\\" + idAsString;
            ProfilesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EVX\\Profiles\\" + idAsString;
            BackupsDirectory = InstallationPath[0] + ":\\EVX\\Backups\\" + idAsString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iPath">Installation Path</param>
        /// <param name="def">Default settings of the game</param>
        /// <param name="regN">Registry name of the game</param>
        public Game(string iPath, IGameDefault def, string regN) : 
            this(iPath, GetDataPath(def.Id, iPath), GetExecutablePath(def.ExecutableName, iPath), 
                def.Name, def.Name_API, def.Name_Nexus, regN, def.Id)
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
            this(iPath, dPath, exePath, def.Name, def.Name_API, def.Name_Nexus, nReg, def.Id)
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
