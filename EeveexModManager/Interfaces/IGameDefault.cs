using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Interfaces
{
    public interface IGameDefault
    {
        GameListEnum Id { get; }
        string Name { get; }
        string Name_Nexus { get; }
        string Name_API { get; }
        string[] Registry_Names { get; }
        string ExecutableName { get; }
    }
}
