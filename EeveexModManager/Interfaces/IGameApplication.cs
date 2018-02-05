using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Interfaces
{
    public interface IGameApplication
    {
        string Name { get; }
        string ExecutablePath { get;}
        GameListEnum AssociatedGameId { get; }
    }
}
