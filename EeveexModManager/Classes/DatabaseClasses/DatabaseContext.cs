using System.Threading;
using LiteDB;

using EeveexModManager.Classes;
using EeveexModManager.Windows;
using EeveexModManager.Services;
using EeveexModManager.Classes.JsonClasses;
using System.IO;
using System.Windows;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class DatabaseContext : LiteDatabase
    {
        public DatabaseContext(Json_Config config) : base(config.Installation_Path + @"\EeveexModManager.db")
        {
            GetCollection<OnlineMod>("online_mods").EnsureIndex(x => x.FileId);
            GetCollection<BaseMod>("offline_mods").EnsureIndex(x => x.FileId);
            GetCollection<Db_Game>("games").EnsureIndex(x => x.Name);

            GetCollection<GameApplication>("game_apps").EnsureIndex(x => x.Name);
        }

        public Game GetCurrentGame()
        {
            return GetCollection<Db_Game>("games").FindOne(x => x.IsCurrent).EncapsulateToSource();
        }

        public void FirstTimeSetup(Mutex m, Service_JsonParser jsp, Json_Config cnfg, NamedPipeManager npm)
        {
            if (GetCollection<OnlineMod>("online_mods").Count() > 0)
                DropCollection("online_mods");
            if (GetCollection<BaseMod>("offline_mods").Count() > 0)
                DropCollection("offline_mods");

            if (GetCollection<Game>("games").Count() > 0)
                DropCollection("games");
            if (GetCollection<GameApplication>("game_apps").Count() > 0)
                DropCollection("game_apps");

            AvailableGamesWindow chooseGames = new AvailableGamesWindow(m, this, jsp, cnfg, npm);
            chooseGames.Show();

            GetCollection<Game>("games").InsertBulk(chooseGames.GetAvailableGames());
        }
    }
}
