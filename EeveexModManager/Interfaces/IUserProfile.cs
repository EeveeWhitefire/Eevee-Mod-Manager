using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Interfaces
{
    public interface IUserProfile
    {
        ulong ProfileId { get;}
        string Name { get; }
        string ProfileDirectory { get; }
        bool PrivateSaves { get; }
        GameListEnum GameId { get; }
    }
}
