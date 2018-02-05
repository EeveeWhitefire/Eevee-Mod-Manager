using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.IO;

using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Classes.JsonClasses;
using EeveexModManager.Services;
using EeveexModManager.Controls;

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for GamePickerWindow.xaml
    /// </summary>
    public partial class GamePickerWindow : Window
    {
        private DatabaseContext _db;
        private Mutex _mutex;
        private Service_JsonParser _jsonParser;
        private Json_Config _config;
        private NamedPipeManager _namedPipeManager;
        private GamePicker_ComboBox _gamePicker;

        public GamePickerWindow(Mutex m, DatabaseContext db, Service_JsonParser jsp, Json_Config cnfg, 
            NamedPipeManager npm)
        {
            _db = db;
            _mutex = m;
            _jsonParser = jsp;
            _config = cnfg;
            _namedPipeManager = npm;

            InitializeComponent();
            _gamePicker = new GamePicker_ComboBox(_db.GetCollection<Db_Game>("games").FindAll(), 80, 25, RerunGameDetection, Temp)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(20, 10, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 450,
                Height = 80
            };

            MainGrid.Children.Add(_gamePicker);
            _gamePicker.SelectedIndex = 0;
        }

        void RerunGameDetection()
        {
            AvailableGamesWindow gameDetectWindow = new AvailableGamesWindow(_mutex, _db, _jsonParser, _config, _namedPipeManager, true, WindowsEnum.GamePickerWindow);
            gameDetectWindow.Show();
            Close();
        }

        void Temp(Game i)
        {

        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Game currGame = _db.GetCollection<Db_Game>("games").FindOne( x => x.IsCurrent).EncapsulateToSource();
            Game selGame = _db.GetCollection<Db_Game>("games").FindAll().ElementAt(_gamePicker.SelectedIndex).EncapsulateToSource();

            if(selGame.Id != currGame.Id)
            {
                selGame.ToggleIsCurrentGame();
                currGame.ToggleIsCurrentGame();
            }

            _db.GetCollection<Db_Game>("games").Update(selGame.EncapsulateToDb());
            _db.GetCollection<Db_Game>("games").Update(currGame.EncapsulateToDb());


            _config.State = StatesOfConfiguartion.Ready;
            _config.Installation_Path = Directory.GetCurrentDirectory();
            _jsonParser.UpdateJson(_config);

            MainWindow mainWindow = new MainWindow(_mutex, _db, _jsonParser, _config, _namedPipeManager);

            mainWindow.Show();
            Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            
            Close();
        }
    }

}
