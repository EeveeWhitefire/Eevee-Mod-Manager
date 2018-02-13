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
        public CheckBox ActivateGameCheckBox { get; protected set; }

        public Mod_Control(Mod mod, DatabaseContext db) : base()
        {
            AssociatedMod = mod;
            Database = db;

            Orientation = Orientation.Horizontal;
            Thickness modListMargin = new Thickness(10, 2, 10, 0);

            ActivateGameCheckBox = new CheckBox()
            {
                Margin = new Thickness(0, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            ActivateGameCheckBox.LayoutTransform = new ScaleTransform(1.5, 1.5);

            ActivateGameCheckBox.Checked += new RoutedEventHandler(ActivateMod);
            ActivateGameCheckBox.Unchecked += new RoutedEventHandler(ActivateMod);
            ActivateGameCheckBox.IsChecked = mod.Active;
            
            Button UninstallModButton = new Button()
            {
                Content = "X",
                Background = Brushes.DarkRed,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Width = 20,
                Height = 20,
                FontSize = 12
            };
            UninstallModButton.Click += new RoutedEventHandler(UninstallMod);


            Children.Add(ActivateGameCheckBox);
            Children.Add(UninstallModButton);
            Children.Add(new TextBlock()
            {
                Text = $"{AssociatedMod.Name} [{AssociatedMod.ModFileName}]",
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 450,
                FontSize = 15
            });
            Children.Add(new TextBlock()
            {
                Text = AssociatedMod.Author,
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 150,
                FontSize = 15
            });
            Children.Add(new TextBlock()
            {
                Text = AssociatedMod.Id.ToString(),
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 100,
                FontSize = 15
            });
            Children.Add(new TextBlock()
            {
                Text = AssociatedMod.Version,
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 100,
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
            AssociatedMod.ToggleIsActive();
            Database.GetCollection<Db_Mod>("mods").Update(AssociatedMod.EncapsulateToDb());

        }
    }
}
