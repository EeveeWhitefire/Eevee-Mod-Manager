using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Controls
{
    public class Mod_Control
    {
        public string ModState { get; set; }
        public string ModName { get; set; }
        public string ModAuthor { get; set; }
        public string ModVersion { get; set; }
        public string ModId { get; set; }

        private DatabaseContext_Profile _db;
        public Mod AssociatedMod { get; set; }

        public Mod_Control(Mod m, DatabaseContext_Profile db)
        {
            AssociatedMod = m;
            _db = db;
            UpdateProperties();
        }

        public void UpdateProperties()
        {
            ModName = AssociatedMod.Name + $" [{AssociatedMod.ModFileName}]";
            ModAuthor = AssociatedMod.Author;
            ModVersion = AssociatedMod.Version;
            ModId = AssociatedMod.Id;
            ModState = AssociatedMod.Active ? "✔" : "🗙";
        }
    }
}
