using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EeveexModManager.Controls
{
    public class UserProfile_Control : ComboBoxItem
    {
        public Db_UserProfile AssociatedProfile { get; protected set; }
        public ProfilesManager ProfileManager { get; protected set; }
        public UserProfile_Control(Db_UserProfile prof, ProfilesManager pm) : base()
        {
            AssociatedProfile = prof;
            ProfileManager = pm;
            Content = AssociatedProfile.Name;
        }
    }
}
