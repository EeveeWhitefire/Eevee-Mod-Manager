using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Interfaces;

namespace EeveexModManager.GameDefaults
{
    public class GameDefaultValues
    {
        public static IDictionary<GameListEnum, IGameDefault> GameDefaults = GetGamesDefault();
        public static Dictionary<GameListEnum, IGameDefault> GetGamesDefault()
        {
            Dictionary<GameListEnum, IGameDefault> dic = new Dictionary<GameListEnum, IGameDefault>();
            foreach (GameListEnum id in Enum.GetValues(typeof(GameListEnum)))
            {
                var def = GetGameDefault(id);
                if (def != null)
                    dic.Add(id, def);
            }

            return dic;
        }

        public static IGameDefault GetGameDefault(GameListEnum id)
        {
            switch (id)
            {
                case GameListEnum.SkyrimSE:
                    return new Default_SkyrimSE();
                case GameListEnum.Skyrim:
                    return new Default_Skyrim();
                case GameListEnum.Oblivion:
                    //return new Default_Oblivion();
                    return null;
                case GameListEnum.Witcher3:
                    return null;
                case GameListEnum.FalloutNV:
                    return new Default_FalloutNV();
                case GameListEnum.Fallout4:
                    return null;
                case GameListEnum.Fallout3:
                    return null;
                case GameListEnum.DragonAge2:
                    return null;
                case GameListEnum.DragonAgeOrigins:
                    return null;
                default:
                    return null;
            }
        }

        public class Default_SkyrimSE : IGameDefault
        {
            public GameListEnum Id { get; } = GameListEnum.SkyrimSE;
            public string Name { get; } = "TESV : Skyrim Special Edition";
            public string Name_Nexus { get; } = "skyrimse";
            public string Name_API { get; } = "skyrimspecialedition";
            public string ExecutableName { get; } = "SkyrimSE.exe";
            public string RelativeDataPath { get; } = "Data";
            public string PluginFileExtension { get; } = "esp";
            public string[] Registry_Names { get; } = new string[]
            { "The Elder Scrolls V: Skyrim Special Edition" };
            public IDictionary<string, string> KnownExecutables { get; } = new Dictionary<string, string>()
            { {"SkyrimSE", "SkyrimSE.exe"}, {"SkyrimSE Laucher", "SkyrimSELauncher.exe" }, {"SKSE", "skse64_loader.exe"} };

        }

        public class Default_Skyrim : IGameDefault
        {
            public GameListEnum Id { get; } = GameListEnum.Skyrim;
            public string Name { get; } = "TESV : Skyrim";
            public string Name_Nexus { get; } = "skyrim";
            public string Name_API { get; } = "skyrim";
            public string RelativeDataPath { get; } = "\\Data";
            public string ExecutableName { get; } = "TESV.exe";
            public string PluginFileExtension { get; } = "esp";
            public string[] Registry_Names { get; } = new string[]
            { "TSEV Skyrim LE", "The Elder Scrolls V: Skyrim" };
            public IDictionary<string, string> KnownExecutables { get; } = new Dictionary<string, string>()
            { {"Skyrim", "TESV.exe"}, {"Skyrim Laucher", "SkyrimLauncher.exe"}, {"SKSE", "skse_loader.exe"} };
        }

        public class Default_Oblivion : IGameDefault
        {
            public GameListEnum Id { get; } = GameListEnum.Oblivion;
            public string Name { get; } = "TESIV : Oblivion";
            public string Name_Nexus { get; } = "skyrim";
            public string Name_API { get; } = "skyrim";
            public string RelativeDataPath { get; } = "\\Data";
            public string ExecutableName { get; } = "SkyrimSE.exe";
            public string PluginFileExtension { get; } = "esp";
            public string[] Registry_Names { get; } = new string[]
            { "TSEV Skyrim LE", "The Elder Scrolls V: Skyrim" };
            public IDictionary<string, string> KnownExecutables { get; } = new Dictionary<string, string>();
        }

        public class Default_FalloutNV : IGameDefault
        {
            public GameListEnum Id { get; } = GameListEnum.FalloutNV;
            public string Name { get; } = "Fallout : New Vegas";
            public string Name_Nexus { get; } = "falloutnv";
            public string Name_API { get; } = "newvegas";
            public string RelativeDataPath { get; } = "\\Data";
            public string ExecutableName { get; } = "FalloutNV.exe";
            public string PluginFileExtension { get; } = "esp";
            public string[] Registry_Names { get; } = new string[] 
            { "Fallout: New Vegas" };
            public IDictionary<string, string> KnownExecutables { get; } = new Dictionary<string, string>()
            { {"FalloutNV", "FalloutNV.exe"}, {"FalloutNV Launcher", "FalloutNVLauncher.exe"} };
        }

        public class Default_Witcher3 : IGameDefault
        {
            public GameListEnum Id { get; } = GameListEnum.Witcher3;
            public string Name { get; } = "The Witcher 3 : Wild Hunt";
            public string Name_Nexus { get; } = "witcher3";
            public string Name_API { get; } = "newvegas";
            public string RelativeDataPath { get; } = "\\Data";
            public string ExecutableName { get; } = "SkyrimSE.exe";
            public string PluginFileExtension { get; } = "esp";
            public string[] Registry_Names { get; } = new string[] 
            { "The Witcher 3" };
            public IDictionary<string, string> KnownExecutables { get; } = new Dictionary<string, string>();
        }
        
        public class Default_DragonAgeOrigins : IGameDefault
        {
            public GameListEnum Id { get; } = GameListEnum.DragonAgeOrigins;
            public string Name { get; } = "Dragon Age Origins";
            public string Name_Nexus { get; } = "dragonage";
            public string Name_API { get; } = "newvegas";
            public string RelativeDataPath { get; } = "\\Data";
            public string ExecutableName { get; } = "SkyrimSE.exe";
            public string PluginFileExtension { get; } = "esp";
            public string[] Registry_Names { get; } = new string[]
            { "Dragon Age Origins" };
            public IDictionary<string, string> KnownExecutables { get; } = new Dictionary<string, string>();
        }

        public class Default_DragonAge2 : IGameDefault
        {
            public GameListEnum Id { get; } = GameListEnum.DragonAge2;
            public string Name { get; } = "Dragon Age II";
            public string Name_Nexus { get; } = "dragonage2";
            public string Name_API { get; } = "newvegas";
            public string RelativeDataPath { get; } = "\\Data";
            public string ExecutableName { get; } = "SkyrimSE.exe";
            public string PluginFileExtension { get; } = "esp";
            public string[] Registry_Names { get; } = new string[] 
            { "Dragon Age II" };
            public IDictionary<string, string> KnownExecutables { get; } = new Dictionary<string, string>();
        }

        public class Default_Fallout3 : IGameDefault
        {
            public GameListEnum Id { get; } = GameListEnum.Fallout3;
            public string Name { get; } = "Fallout 3";
            public string Name_Nexus { get; } = "dragonage2";
            public string Name_API { get; } = "newvegas";
            public string RelativeDataPath { get; } = "Data";
            public string ExecutableName { get; } = "SkyrimSE.exe";
            public string PluginFileExtension { get; } = "esp";
            public string[] Registry_Names { get; } = new string[] 
            { "Fallout3" };
            public IDictionary<string, string> KnownExecutables { get; } = new Dictionary<string, string>();
        }

        public class Default_Fallout4 : IGameDefault
        {
            public GameListEnum Id { get; } = GameListEnum.Fallout4;
            public string Name { get; } = "Fallout 4";
            public string Name_Nexus { get; } = "dragonage2";
            public string Name_API { get; } = "newvegas";
            public string RelativeDataPath { get; } = "\\Data";
            public string ExecutableName { get; } = "SkyrimSE.exe";
            public string PluginFileExtension { get; } = "esp";
            public string[] Registry_Names { get; } = new string[]
            { "Fallout 4" };
            public IDictionary<string, string> KnownExecutables { get; } = new Dictionary<string, string>();
        }
    }
}
