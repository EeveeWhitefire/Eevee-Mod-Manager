using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteDB;

using EeveexModManager.Interfaces;
using EeveexModManager.Classes;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class UserProfile : IUserProfile
    {
        [BsonId]
        public ulong ProfileId { get; set; }

        public string Name { get; set; }
        public string ProfileDirectory { get; set; }
        public bool PrivateSaves { get; set; } = false;
        public GameListEnum GameId { get; set; }

        #region Constructors
        public UserProfile() { }

        public UserProfile(string n, GameListEnum gameId, bool usesPrivateSaves, string dir, ulong id)
        {
            Name = n;
            PrivateSaves = usesPrivateSaves;
            GameId = gameId;
            ProfileDirectory = dir;
            ProfileId = id;
        }
        #endregion
    }
}
