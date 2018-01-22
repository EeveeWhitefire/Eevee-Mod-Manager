
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using LiteDB;

using EeveexModManager.Classes;
using EeveexModManager.Windows;

namespace EeveexModManager
{
    public class DatabaseContext : LiteDatabase
    {
        public DatabaseContext() : base("EeveexModManager.db")
        {
            GetCollection<OnlineMod>("online_mods");
            GetCollection<BaseMod>("offline_mods");
            GetCollection<Game>("games");
            GetCollection<GameApplication>("game_apps");
        }

        public void CheckIfFreshInstall(bool isFirstTime)
        {
            if (isFirstTime)
            {
                FirstTimeSetup();
            }
        }
        /*
        LiteDatabase GetDatabase()
        {
            var db = new LiteDatabase();
            // Get collection instance
            var col = db.GetCollection<OnlineMod>("customer");

            // Insert document to collection - if collection do not exits, create now
            col.Insert(new Customer { Id = 1, Name = "John Doe" });

            // Create, if not exists, new index on Name field
            col.EnsureIndex(x => x.Name);

            // Now, search for document your document
            var customer = col.FindOne(x => x.Name == "john doe");

            return db;
        }*/
        public Game GetCurrentGame()
        {
            return GetCollection<Game>("games").FindOne(x => x.IsCurrent);
        }

        void FirstTimeSetup()
        {
            if (GetCollection<OnlineMod>("online_mods").Count() > 0)
                DropCollection("online_mods");
            if(GetCollection<BaseMod>("offline_mods").Count() > 0)
                DropCollection("offline_mods");

            if (GetCollection<Game>("games").Count() > 0)
                DropCollection("games");
            if (GetCollection<GameApplication>("game_apps").Count() > 0)
                DropCollection("game_apps");

            AvailableGamesWindow chooseGames = new AvailableGamesWindow();
            chooseGames.Show();

            GetCollection<Game>("games").InsertBulk(chooseGames.GetAvailableGames());
        }
    }
}
