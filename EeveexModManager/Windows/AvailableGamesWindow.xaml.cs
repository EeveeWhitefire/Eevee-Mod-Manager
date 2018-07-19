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
using EeveexModManager.Controls;
using EeveexModManager.Services;
using EeveexModManager.Interfaces;
using EeveexModManager.Classes.GameDefaults;


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
        private NamedPipeManager _namedPipeManager;
        private Mutex _mutex;
        private List<Border> GameDetectorControlBorders;
        private IEnumerable<IGameDefault> _gameDefaults = GameDefaultValues.GetGamesDefault();

        private int GamesConfigured = 0;

        private WindowsEnum GetBackTo = WindowsEnum.GameDetectionWindow;

        public AvailableGamesWindow(Mutex m, DatabaseContext_Main db, Service_JsonParser jsp,
            NamedPipeManager npm, List<DatabaseContext_Profile> profiles, ProfilesManager profMng, 
            bool getBackButton = false, WindowsEnum backTo = WindowsEnum.MainWindow)
        {
            _mutex = m;
            _db = db;
            _jsonParser = jsp;
            _namedPipeManager = npm;
            _dbProfiles = profiles;
            _profilesManager = profMng;

            _gameSeachers = new List<GameSearcher>();
            _games = new List<Game>();

            InitializeComponent();
            GameDetectorControlBorders = (GameSearchers_StackPanel.Children.Cast<StackPanel>())
                .SelectMany(x => x.Children.Cast<Border>()).ToList();

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
                    MainWindow mainWindow = new MainWindow(_mutex, _db, _jsonParser,
                        _namedPipeManager, _dbProfiles, _profilesManager);
                    mainWindow.Show();
                    break;
            }
            Close();
        }

        const int GamesPerTab = 4;

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
                InitGames(_games);
            }
            else
            {
                MessageBox.Show("Error! No games were found! Please click on the \"Restart Scans\" button to retry!");
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

                Defined.Settings.State = StatesOfConfiguration.Ready;

                MainWindow window = new MainWindow(_mutex, _db, _jsonParser, _namedPipeManager, _dbProfiles, _profilesManager);
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

        private void AddGameDetectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameDefaults.Count() > 0)
            {
                Border parent = (sender as Button).Parent as Border;
                GameDetectionAdderWindow window = new GameDetectionAdderWindow(_gameDefaults,
                    parent, ConfirmGameDetection);
                window.Show();
            }
        }

        private void ConfirmGameDetection(IGameDefault g, Border parent)
        {
            parent.Dispatcher.Invoke(() =>
            {
                (_gameDefaults as IList<IGameDefault>).Remove(g);

                if (_gameDefaults.Count() > 0)
                {
                    int indexOfBorder = GameDetectorControlBorders.IndexOf(parent);
                    if (indexOfBorder < GameDetectorControlBorders.Count - 1)
                    {
                        GameDetectorControlBorders.ElementAt(indexOfBorder + 1).Visibility = Visibility.Visible;
                    }
                }
                GameDetector_Control currDetectorControl = new GameDetector_Control(g);

                _gameSeachers.Add(new GameSearcher(g, currDetectorControl, _games));
                currDetectorControl.Searcher = _gameSeachers.LastOrDefault();
                currDetectorControl.Dispatcher.BeginInvoke((Action)(() => currDetectorControl.Searcher.StartSearch()), DispatcherPriority.Background);

                parent.Child = currDetectorControl;
            });

            GC.Collect();
        }
    }
}
