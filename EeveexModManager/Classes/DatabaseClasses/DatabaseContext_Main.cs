using System.Threading;
using System.IO;
using System.Windows;
using System.Linq;

using LiteDB;

using EeveexModManager.Classes;
using EeveexModManager.Windows;
using EeveexModManager.Services;
using System.Collections.Generic;
using System;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class DatabaseContext_Main : DatabaseContextBase
    {
        public DatabaseContext_Main() : base(Defined.Settings.ApplicationDataPath + @"\EeveeModManager.db4")
        {
            GetCollection<Game>("games").EnsureIndex(x => x.Name);

            GetCollection<UserProfile>("profiles").EnsureIndex(x => x.ProfileId);
            GetCollection<GameApplication>("game_apps").EnsureIndex(x => x.Name);

        }

        public Game GetCurrentGame()
        {
            return GetCollection<Game>("games").FindOne(x => x.IsCurrent);
        }

        public void UpdateCurrentGame(Game newCurr, Action afterSetGame)
        {
            Game curr = GetCurrentGame();
            if (newCurr.Id != curr.Id)
            {
                curr.ToggleIsCurrentGame();
                newCurr.ToggleIsCurrentGame();
                GetCollection<Game>("games").Update(curr);
                GetCollection<Game>("games").Update(newCurr);
            }
            afterSetGame();
        }


        [STAThread]
        public void FirstTimeSetup(Mutex m, Service_JsonParser jsp, 
            NamedPipeManager npm, List<DatabaseContext_Profile> profiles, ProfilesManager profileManager)
        {
            if (GetCollection<Game>("games").Count() > 0)
                GetCollection<Game>("games").Delete( x => true);

            if (GetCollection<GameApplication>("game_apps").Count() > 0)
                GetCollection<GameApplication>("game_apps").Delete(x => true);
            foreach (var item in profiles)
            {
                item.Dispose();
            }
            profiles.Clear();

            foreach (var item in GetCollection<UserProfile>("profiles").FindAll())
            {
                profileManager.DeleteProfile(item.ProfileId);
            }

            AvailableGamesWindow chooseGames = new AvailableGamesWindow(m, this, jsp, npm, profiles, profileManager);
            chooseGames.Show();
        }
    }
}
