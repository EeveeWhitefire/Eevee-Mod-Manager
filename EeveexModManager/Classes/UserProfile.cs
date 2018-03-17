using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Interfaces;
using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Classes
{
    public class UserProfile : IUserProfile
    {
        public ulong ProfileId { get; protected set; }
        public string Name { get; protected set; }
        public string ProfileDirectory { get; protected set; }
        public bool PrivateSaves { get; protected set; } = false;
        public GameListEnum GameId { get; protected set; }

        public UserProfile(string n, GameListEnum gameId, bool usesPrivateSaves, string dir, ulong id)
        {
            Name = n;
            PrivateSaves = usesPrivateSaves;
            GameId = gameId;
            ProfileDirectory = dir;
            ProfileId = id;
        }

        public Db_UserProfile EncapsulateToDb()
        {
            return new Db_UserProfile()
            {
                Name = Name,
                GameId = GameId,
                PrivateSaves = PrivateSaves,
                ProfileDirectory = ProfileDirectory,
                ProfileId = ProfileId
            };
        }
    }
}
