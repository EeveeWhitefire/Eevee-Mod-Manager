using System.Threading;
using System.IO;
using System.Windows;

using LiteDB;

using EeveexModManager.Classes;
using EeveexModManager.Windows;
using EeveexModManager.Services;
using EeveexModManager.Interfaces;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class DatabaseContext_Profile : LiteDatabase
    {
        public GameListEnum GameId { get; protected set; }

        public DatabaseContext_Profile(string profileDir, GameListEnum gameId) : 
            base(profileDir + $@"\EVX.db")
        {
            GameId = GameId;
            GetCollection<Db_Mod>("mods").EnsureIndex(x => x.FileId);
        }

        public void FirstTimeSetup()
        {
            if (GetCollection<Db_Mod>("mods").Count() > 0)
                DropCollection("mods");
        }
    }
}