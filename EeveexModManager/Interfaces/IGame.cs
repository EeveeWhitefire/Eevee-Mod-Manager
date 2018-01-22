using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Interfaces
{
    public enum GameListEnum
    {
        SkyrimSE,
        Skyrim,
        FalloutNV,
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

        GameListEnum Id { get;}
        string IconName { get;}

        bool IsCurrent { get; }
    }
}
