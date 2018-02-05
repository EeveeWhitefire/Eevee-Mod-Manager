using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Classes;
using EeveexModManager.Interfaces;
using LiteDB;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class Db_GameApplication : IGameApplication
    {
        [BsonId]
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public GameListEnum AssociatedGameId { get; set; }


        public GameApplication EncapsulateToSource()
        {
            return new GameApplication(Name, ExecutablePath, AssociatedGameId);
        }
    }
}
