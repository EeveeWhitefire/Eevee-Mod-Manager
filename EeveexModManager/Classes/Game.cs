using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Interfaces;
using Microsoft.Win32;

namespace EeveexModManager.Classes
{
    public class Game : IGame
    {
        #region Fields
        public string DataPath { get; }
        public string InstallationPath { get; }
        public string ExecutablePath { get; }

        public string Name { get; }
        public string Name_API { get; }
        public string Name_Registry { get; protected set; }
        public string ModsDirectory { get; protected set; }
        public string DownloadsDirectory { get; protected set; }

        public GameListEnum Id { get; }
        public bool IsCurrent { get; protected set; } = false;
        #endregion
        #region Static Methods

        static string GetExecutablePath(string n, string path)
        {
            return ProcessDirectory(path, n);
        }
        public static string[] GetRegistryName(string n)
        {
            switch (n)
            {
                case "TESV : Skyrim Special Edition":
                    return new string[] { "The Elder Scrolls V: Skyrim Special Edition" };
                case "TESV : Skyrim":
                    return new string[] { "TSEV Skyrim LE", "The Elder Scrolls V: Skyrim" };
                case "Fallout : New Vegas":
                    return new string[] { "Fallout: New Vegas" };
                case "Fallout 4":
                    return new string[] { "Fallout 4" };
                case "Fallout 3":
                    return new string[] { "Fallout3" };
                case "Dragon Age II":
                    return new string[] { "Dragon Age II" };
                default:
                    return new string[] { };
            }
        }
        public static string GetDataPath(string n, string path)
        {
            switch (n)
            {
                case "TESV : Skyrim Special Edition":
                    return path + @"\Data";
                case "TESV : Skyrim":
                    return path + @"\Data";
                case "Fallout : New Vegas":
                    return path + @"\Data";
                case "Fallout 4":
                    return path + @"\Data";
                case "Fallout 3":
                    return path + @"\Data";
                case "Dragon Age II":
                    return string.Empty;
                default:
                    return string.Empty;
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
                return "";
            }
            else
            {
                return "";
            }
        }

        #endregion

        
        public static Game CreateByName(string n, string iPath, string regN)
        {
            switch (n)
            {
                case "TESV : Skyrim Special Edition":
                    return new Game(iPath, GetDataPath(n, iPath), GetExecutablePath("SkyrimSE.exe", iPath), n, "skyrimse", regN, GameListEnum.SkyrimSE);
                case "TESV : Skyrim":
                    return new Game(iPath, GetDataPath(n, iPath), GetExecutablePath("Skyrim.exe", iPath), n, "skyrim", regN, GameListEnum.Skyrim);
                case "Fallout : New Vegas":
                    return new Game(iPath, GetDataPath(n, iPath), GetExecutablePath("FalloutNV.exe", iPath), n, "newvegas", regN, GameListEnum.FalloutNV);
                case "Fallout 4":
                    return null;
                case "Fallout 3":
                    return null;
                case "Dragon Age II":
                    return null;
                default:
                    return null;
            }
        }

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
        public Game(string iPath, string dPath, string exePath, string n, string nApi, string nReg, GameListEnum id)
        {
            InstallationPath = iPath;
            DataPath = dPath;
            ExecutablePath = exePath;
            Name_API = nApi;
            Id = id;
            Name_Registry = nReg;
            Name = n;
            ModsDirectory = @"Mods\" + Name.Replace(':', '-');
            DownloadsDirectory = @"Downloads\" + Name.Replace(':', '-');
        }
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
        public Game(string iPath, string dPath, string exePath, string n, string nApi, string nReg, GameListEnum id, bool isCurr) : this(iPath, dPath, exePath, n, nApi, nReg, id)
        {
            IsCurrent = isCurr;
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
                Name_API = Name_API,
                Name_Registry = Name_Registry
            };
        }
    }
}
