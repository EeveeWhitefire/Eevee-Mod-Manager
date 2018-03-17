using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Classes.JsonClasses;
using EeveexModManager.Controls;
using EeveexModManager.Services;



namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for AvailableGamesWindow.xaml
    /// </summary>
    public partial class AvailableGamesWindow : Window
    {
        private List<GameSearcher> _gameSeachers;
        private List<Game> _games;
        private List<DatabaseContext_Profile> _dbProfiles;
        private DatabaseContext_Main _db;
        private ProfilesManager _profilesManager;
        private Service_JsonParser _jsonParser;
        private Json_Config _config;
        private NamedPipeManager _namedPipeManager;
        private Mutex _mutex;
        private GamePicker_ComboBox _gamePicker;

        private int GamesConfigured = 0;

        private WindowsEnum GetBackTo = WindowsEnum.GameDetectionWindow;

        public AvailableGamesWindow(Mutex m, DatabaseContext_Main db, Service_JsonParser jsp, Json_Config cnfg,
            NamedPipeManager npm, List<DatabaseContext_Profile> profiles, ProfilesManager profMng, 
            bool getBackButton = false, WindowsEnum backTo = WindowsEnum.MainWindow)
        {
            _mutex = m;
            _db = db;
            _jsonParser = jsp;
            _config = cnfg;
            _namedPipeManager = npm;
            _dbProfiles = profiles;
            _profilesManager = profMng;
            _gamePicker = new GamePicker_ComboBox(80, 25)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20, 10, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 450,
                Height = 80,
                SelectedIndex = 0
            };

            InitializeComponent();
            StartSearches();

            if(getBackButton)
            {
                BackToMain_Button.Visibility = Visibility.Visible;
                GetBackTo = backTo;
            }

        }

        private void GetBackToMainWindow_Click(object sender, RoutedEventArgs e)
        {
            switch (GetBackTo)
            {
                case WindowsEnum.MainWindow:
                    MainWindow mainWindow = new MainWindow(_mutex, _db, _jsonParser, _config, 
                        _namedPipeManager, _dbProfiles, _profilesManager);
                    mainWindow.Show();
                    break;
            }
            Close();
        }

        const int GamesPerTab = 4;

        void StartSearches()
        {
           int counter = 0;
            string[] gameNames = new string[]
                {"TESV : Skyrim Special Edition", "TESV : Skyrim", "Fallout 4", "Fallout : New Vegas",
                     "Fallout 3", "Dragon Age Origins", "Dragon Age II", "The Witcher 3 : Wild Hunt"};

            _gameSeachers = new List<GameSearcher>();
            _games = new List<Game>();

            foreach (StackPanel item in GameSearchers_StackPanel.Children)
            {
                var names = gameNames.Skip(counter);
                counter = 0;
                foreach (var x in names)
                {
                    Border border = new Border()
                    {
                        BorderBrush = Brushes.Gainsboro,
                        BorderThickness = new Thickness(2),
                        Margin = new Thickness(0, 0, 0, 5),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    GameDetector_Control currDetectorControl = new GameDetector_Control(x, _gamePicker, GamePicker_StkPanel);

                    _gameSeachers.Add(new GameSearcher(x, currDetectorControl));
                    currDetectorControl.Searcher = _gameSeachers.LastOrDefault();
                    currDetectorControl.Dispatcher.BeginInvoke((Action)( () => currDetectorControl.Searcher.StartSearch()), DispatcherPriority.Background);

                    border.Child = currDetectorControl;

                    item.Children.Add(border);
                    counter++;
                    if (counter >= GamesPerTab)
                    {
                        break;
                    }
                }
            }
        }
        private void RestartScansButton_Click(object sender, RoutedEventArgs e)
        {
            if(_gameSeachers.Where( x => x.Search).Count() > 0)
            {
                MessageBox.Show("Error! There are still scans commencing! Please wait till each one of them has ended or click on the \"Ignore All\" button!");
            }
            else
            {
                _gameSeachers.ForEach(x =>
               {
                   Confirm_Button.Dispatcher.BeginInvoke((Action)(() => x.RestartSearch()), DispatcherPriority.Background);
               });
            }
        }

        void InitGames(List<Game> _games)
        {
            _games.ForEach(x =>
            {
                GameConfigurationWindow win = new GameConfigurationWindow(x, ConfiguartionPart2);
                win.Show();
            });
        }

        private void ConfirmGamesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameSeachers.Where( x => x.Confirmed).Count() > 0)
            {
                _games = _gameSeachers.Where(x => x.Confirmed)
                    .Select(x => x.AssociatedGame).ToList();

                _games.ElementAt(_gamePicker.SelectedIndex).ToggleIsCurrentGame();

                InitGames(_games);
            }
            else
            {
                MessageBox.Show("Error! No _games were found! Please click on the \"Restart Scans\" button to retry!");
            }
        }

        private void ConfiguartionPart2()
        {
            GamesConfigured++;
            if(GamesConfigured == _games.Count)
            {
                _db.GetCollection<Db_Game>("games").Delete(x => true);
                _db.GetCollection<Db_GameApplication>("game_apps").Delete(x => true);
                _db.GetCollection<Db_UserProfile>("profiles").Delete(x => true);

                _db.GetCollection<Db_Game>("games").InsertBulk(_games.Select(x => x.EncapsulateToDb()));

                var gameApps = _games.Select(x => x.AutoDetectApplications()).SelectMany(x => x).Select(y => y.EncapsulateToDb());

                _db.GetCollection<Db_GameApplication>("game_apps").InsertBulk(gameApps);

                foreach (var item in _games)
                {
                    _profilesManager.AddProfile("master", item);
                    _dbProfiles.Add(new DatabaseContext_Profile(item.ProfilesDirectory + "\\master", item.Id));
                }

                Game currGame = _db.GetCollection<Db_Game>("games").FindOne(x => x.IsCurrent).EncapsulateToSource();
                Game selGame = _db.GetCollection<Db_Game>("games").FindAll().ElementAt(_gamePicker.SelectedIndex).EncapsulateToSource();

                if (selGame.Id != currGame.Id)
                {
                    selGame.ToggleIsCurrentGame();
                    currGame.ToggleIsCurrentGame();
                }

                _db.GetCollection<Db_Game>("games").Update(selGame.EncapsulateToDb());
                _db.GetCollection<Db_Game>("games").Update(currGame.EncapsulateToDb());


                _config.State = StatesOfConfiguartion.Ready;
                _config.Installation_Path = Directory.GetCurrentDirectory();
                _jsonParser.UpdateJson(_config);

                MainWindow window = new MainWindow(_mutex, _db, _jsonParser,
                    _config, _namedPipeManager, _dbProfiles, _profilesManager);
                window.Show();

                Close();
            }
        }

        private void IgnoreAllModsButton_Click(object sender, RoutedEventArgs e)
        {
            _gameSeachers.ForEach(x => x.GuiControl.IgnoreMod());
        }

        private void ConfirmAllGamesButton_Click(object sender, RoutedEventArgs e)
        {
            _gameSeachers.Where(x => x.Exists).ToList().ForEach( x =>
            {
                x.GuiControl.ConfirmGame();
            });
        }
    }
}
