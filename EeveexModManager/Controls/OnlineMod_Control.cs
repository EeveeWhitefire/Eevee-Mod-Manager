using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Controls
{
    public class OnlineMod_Control : Mod_Control
    {
        public OnlineMod AssociatedOnlineMod { get; protected set; }
        public OnlineMod_Control(OnlineMod mod, int index, DatabaseContext db) : base(mod, index, db)
        {
            AssociatedOnlineMod = mod;
        }

        protected override void ActivateMod(object sender, RoutedEventArgs e)
        {
            if (AssociatedMod.Active)
            {
                ActivateGameButton.Background = Brushes.Red;
            }
            else
            {
                ActivateGameButton.Background = Brushes.Green;
            }

            AssociatedMod.ToggleIsActive();
            Database.GetCollection<Db_OnlineMod>("online_mods").Update(AssociatedOnlineMod.EncapsulateToDb_Online());
        }
    }
}
