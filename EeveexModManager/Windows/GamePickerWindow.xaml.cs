using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

using EeveexModManager.Classes;
using EeveexModManager.Classes.JsonClasses;
using EeveexModManager.Services;
using System.IO;

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for GamePickerWindow.xaml
    /// </summary>
    public partial class GamePickerWindow : Window
    {
        DatabaseContext _db;
        Mutex mutex;
        Service_JsonParser _jsonParser;
        Json_Config _config;
        NamedPipeManager namedPipeManager;

        public GamePickerWindow(ref Mutex m, ref DatabaseContext db, ref Service_JsonParser jsp, ref Json_Config cnfg, ref NamedPipeManager npm)
        {
            _db = db;
            mutex = m;
            _jsonParser = jsp;
            _config = cnfg;
            namedPipeManager = npm;

            InitializeComponent();

            db.GetCollection<Game>("games").FindAll().ToList().ForEach(x =>
            {
                AddGame(x);
            });
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            List<Game> updatedGameList = _db.GetCollection<Game>("games").FindAll().ToList();

            updatedGameList.ForEach(x =>
            {
               if (x.IsCurrent || updatedGameList.IndexOf(x) == GameDropdown.SelectedIndex)
               {
                   x.ToggleCurrent();
               }
            });

            _db.GetCollection<Game>("games").Update(updatedGameList);

            
            _config.First_Time = false;
            _config.Installation_Path = Directory.GetCurrentDirectory();
            _jsonParser.UpdateJson(_config);

            MainWindow mainWindow = new MainWindow(ref mutex, ref _db, ref _jsonParser, ref _config, ref namedPipeManager);

            mainWindow.Show();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            
            Close();
        }

        void AddGame(Game game)
        {
            StackPanel x = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };

            Image icon = new Image
            {
                Width = 80,
                Margin = new Thickness(10, 0, 40, 0),
                Source = (BitmapImage)FindResource(game.IconName)
            };

            TextBlock txtBlock = new TextBlock()
            {
                Text = game.Name,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 30
            };

            x.Children.Add(icon);
            x.Children.Add(txtBlock);

            GameDropdown.Items.Add(x);
        }
    }

}
