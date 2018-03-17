using System.Threading;
using System.IO;
using System.Windows;

using LiteDB;

using EeveexModManager.Classes;
using EeveexModManager.Windows;
using EeveexModManager.Services;
using EeveexModManager.Classes.JsonClasses;
using System.Collections.Generic;
using System;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class DatabaseContext_Main : LiteDatabase
    {
        public DatabaseContext_Main(Json_Config config) : base(config.AppData_Path + @"\EeveexModManager.db")
        {
            GetCollection<Db_Game>("games").EnsureIndex(x => x.Name);

            GetCollection<Db_UserProfile>("profiles").EnsureIndex(x => x.ProfileId);
            GetCollection<Db_GameApplication>("game_apps");

        }

        public Game GetCurrentGame()
        {
            return GetCollection<Db_Game>("games").FindOne(x => x.IsCurrent).EncapsulateToSource();
        }

        public void FirstTimeSetup(Mutex m, Service_JsonParser jsp, Json_Config cnfg, 
            NamedPipeManager npm, List<DatabaseContext_Profile> profiles, ProfilesManager profileManager)
        {
            if (GetCollection<Game>("games").Count() > 0)
                GetCollection<Game>("games").Delete( x => true);

            if (GetCollection<Db_GameApplication>("game_apps").Count() > 0)
                GetCollection<Db_GameApplication>("game_apps").Delete(x => true);

            profiles.ForEach(x =>
            {
                x.Dispose();
            });
            profiles.Clear();
            GC.Collect();
            foreach (var item in GetCollection<Db_UserProfile>("profiles").FindAll())
            {
                profileManager.DeleteProfile(item.ProfileId);
            }

            AvailableGamesWindow chooseGames = new AvailableGamesWindow(m, this, jsp, cnfg, npm, 
                profiles, profileManager);
            chooseGames.Show();
        }
    }
}
