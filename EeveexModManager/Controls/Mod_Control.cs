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
    public class Mod_Control : StackPanel
    {
        public BaseMod AssociatedMod { get; protected set; }
        public DatabaseContext Database { get; protected set; }
        public Button ActivateGameButton { get; protected set; }

        public Mod_Control(BaseMod mod, int index, DatabaseContext db) : base()
        {
            AssociatedMod = mod;
            Database = db;

            Orientation = Orientation.Horizontal;
            Thickness modListMargin = new Thickness(10, 2, 10, 0);

            ActivateGameButton = new Button()
            {
                Background = AssociatedMod.Active ? Brushes.Green : Brushes.Red,
                Content = index,
                Margin = modListMargin,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 20,
                Height = 25,
                FontSize = 15
            };
            ActivateGameButton.Click += new RoutedEventHandler(ActivateMod);

            Children.Add(ActivateGameButton);
            Children.Add(new TextBlock()
            {
                Text = AssociatedMod.Name,
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 250,
                FontSize = 15
            });
            Children.Add(new TextBlock()
            {
                Text = AssociatedMod.Id.ToString(),
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 50,
                FontSize = 15
            });
            Children.Add(new TextBlock()
            {
                Text = AssociatedMod.Version,
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 30,
                FontSize = 15
            });
        }

        protected virtual void ActivateMod(object sender, RoutedEventArgs e)
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
            Database.GetCollection<Db_BaseMod>("offline_mods").Update(AssociatedMod.EncapsulateToDb());
        }
    }
}
