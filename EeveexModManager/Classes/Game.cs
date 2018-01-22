using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EeveexModManager.Interfaces;
using Microsoft.Win32;

namespace EeveexModManager.Classes
{
    public class Game : IGame
    {
        [Key]
        public int DbIndex { get; protected set; }

        public string DataPath { get; }
        public string InstallationPath { get; }
        public string ExecutablePath { get; }

        public string Name { get; }
        public string Name_API { get; }
        public string Name_Registry { get; protected set; }

        public GameListEnum Id { get; }
        public string IconName { get; }
        public bool IsCurrent { get; protected set; } = false;

        public static Game CreateByName(string n, string dPath, string regN)
        {
            switch (n)
            {
                case "TESV : Skyrim Special Edition":
                    return new Game(dPath, GameSearcher.GetExecutablePath("SkyrimSE.exe"), n, "skyrimse", regN, GameListEnum.SkyrimSE);
                case "TESV : Skyrim":
                    return new Game(dPath, GameSearcher.GetExecutablePath("Skyrim.exe"), n, "skyrim", regN, GameListEnum.Skyrim);
                case "Fallout : New Vegas":
                    //return "falloutnv";
                    return null;
                case "Fallout 4":
                    return null;
                case "Fallout 3":
                    return null;
                default:
                    return null;
            }
        }

        public static string[] GetRegistryName(string n)
        {
            switch (n)
            {
                case "TESV : Skyrim Special Edition":
                    return new string[] { "skyrimse" };
                case "TESV : Skyrim":
                    return new string[] { "TSEV Skyrim LE" };
                case "Fallout : New Vegas":
                    //return "falloutnv";
                    return null;
                case "Fallout 4":
                    return new string[] { "Fallout 4" };
                case "Fallout 3":
                    return new string[] {"Fallout3"};
                default:
                    return null;
            }
        }

        public Game(string dPath, string exeName, string n, string nApi, string nReg, GameListEnum id)
        {
            InstallationPath = dPath;
            ExecutablePath = exeName;
            Name_API = nApi;
            Id = id;
            Name_Registry = nReg;
        }


        public void ToggleCurrent()
        {
            IsCurrent = !IsCurrent;
        }
    }
}
