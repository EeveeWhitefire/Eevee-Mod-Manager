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
    public class Db_UserProfile : IUserProfile
    {
        [BsonId]
        public ulong ProfileId { get; set; }

        public string Name { get; set; }
        public string ProfileDirectory { get; set; }
        public bool PrivateSaves { get; set; } = false;
        public GameListEnum GameId { get; set; }

        public UserProfile EncapsulateToSource()
        {
            return new UserProfile(Name, GameId, PrivateSaves, ProfileDirectory, ProfileId);
        }
    }
}
