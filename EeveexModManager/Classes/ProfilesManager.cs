using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Interfaces;
using System.Windows;

namespace EeveexModManager.Classes
{
    public class ProfilesManager
    {
        private DatabaseContext_Main _mainDb;

        public ProfilesManager(DatabaseContext_Main db)
        {
            _mainDb = db;
        }

        public void DeleteProfile(ulong profileId)
        {
            var profile = _mainDb.GetCollection<UserProfile>("profiles").FindOne(x => x.ProfileId == profileId);
            if(profile != null)
            {
                
                if (Directory.Exists(profile.ProfileDirectory))
                    Directory.Delete(profile.ProfileDirectory, true);

                _mainDb.GetCollection<UserProfile>("profiles").Delete(x => x.ProfileId == profileId);
            }
        }

        public void AddProfile(string name, Game g, bool privateSaves = false)
        {
            var profile = _mainDb.GetCollection<DatabaseClasses.UserProfile>("profiles")
                .FindOne(x => x.GameId == g.Id && x.Name.ToLower() == name.ToLower());
            if (profile != null)
            {
                MessageBox.Show("Error! There is already a profile with the same name configured for the same game!");
            }
            else
            {
                var invalidChars = name.ToCharArray().Intersect(Path.GetInvalidPathChars());
                if (invalidChars.Count() > 0)
                {
                    MessageBox.Show($"Error! Invalid characters in name!\n{string.Join(", ", invalidChars)}");
                }
                else
                {
                    string profileDir = g.ProfilesDirectory + $"\\{name}";
                    DatabaseClasses.UserProfile prof = new DatabaseClasses.UserProfile
                    {
                        ProfileDirectory = profileDir,
                        GameId = g.Id,
                        Name = name,
                        PrivateSaves = privateSaves
                    };
                    if (!Directory.Exists(profileDir))
                        Directory.CreateDirectory(profileDir);
                    _mainDb.GetCollection<DatabaseClasses.UserProfile>("profiles").Insert(prof);
                }
            }
        }
    }
}
