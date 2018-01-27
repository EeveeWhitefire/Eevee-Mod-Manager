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
        FalloutNV = 130,
        Fallout4
    }

    public interface IGame
    {
        string DataPath { get; }
        string InstallationPath { get; }
        string ExecutablePath { get;}

        string Name { get;}
        string Name_API { get;}
        string Name_Registry { get; }
        string ModsDirectory { get; }
        string DownloadsDirectory { get;}

        GameListEnum Id { get;}

        bool IsCurrent { get; }
    }
}
