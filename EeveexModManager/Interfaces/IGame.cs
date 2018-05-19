using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Interfaces
{
    public enum GameListEnum
    {
        SkyrimSE = 1704,
        Skyrim,
        Oblivion,
        Witcher3,
        FalloutNV = 130,
        Fallout4,
        Fallout3,
        DragonAge2,
        DragonAgeOrigins
    }

    public interface IGame
    {
        string DataPath { get; }
        string InstallationPath { get; }
        string ExecutablePath { get; }
        string Name_Registry { get; }

        string ModsDirectory { get; }
        string DownloadsDirectory { get; }
        string ProfilesDirectory { get; }
        string BackupsDirectory { get; }
        bool IsCurrent { get; }
    }
}
