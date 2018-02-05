using System;
using System.Collections.Generic;
using System.IO;
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
        public Mod AssociatedMod { get; protected set; }
        public DatabaseContext Database { get; protected set; }
        public Button ActivateGameButton { get; protected set; }

        public Mod_Control(Mod mod, int index, DatabaseContext db) : base()
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

            Button UninstallModButton = new Button()
            {
                Content = "X",
                Background = Brushes.DarkRed,
                Foreground = Brushes.White,
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 20,
                Height = 25,
                FontSize = 15
            };
            UninstallModButton.Click += new RoutedEventHandler(UninstallMod);


            Children.Add(UninstallModButton);
            Children.Add(ActivateGameButton);
            Children.Add(new TextBlock()
            {
                Text = $"{AssociatedMod.Name} [{AssociatedMod.ModFileName}]",
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 400,
                FontSize = 15
            });
            Children.Add(new TextBlock()
            {
                Text = $"By: {AssociatedMod.Author}",
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 100,
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
        private void UninstallMod(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show($"Are you sure you'd like to uninstall the mod:\n\n{AssociatedMod.Name} [{AssociatedMod.ModFileName}]       ???", "Authorization Method", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                if (Directory.Exists(AssociatedMod.ModDirectory))
                    Directory.Delete(AssociatedMod.ModDirectory, true);

                if(Directory.Exists(AssociatedMod.DownloadDirectory))
                    Directory.Delete(AssociatedMod.DownloadDirectory, true);


                Database.GetCollection<Db_Mod>("mods").Delete(x => x.FileId == AssociatedMod.FileId);
                (Parent as TreeView).Items.Remove(this);
            }
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
            Database.GetCollection<Db_Mod>("mods").Update(AssociatedMod.EncapsulateToDb());
        }
    }
}
