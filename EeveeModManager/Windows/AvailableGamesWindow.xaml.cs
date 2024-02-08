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
using EeveexModManager.GameDefaults;


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
        private IEnumerable<IGameDefault> _gameDefaults = GameDefaultValues.GetGamesDefault();
        private ConfigurationPurpose _purpose;
        private Action _uponConfigured = null;
        
        public enum ConfigurationPurpose
        {
            FirstTime,
            AddGames
        }

        public AvailableGamesWindow(DatabaseContext_Main db,  ConfigurationPurpose purpose = ConfigurationPurpose.AddGames, Action uponConfigured = null)
        {
            InitializeComponent();

            _db = db;
            _purpose = purpose;
            _uponConfigured = uponConfigured;
            var games = _db.GetAll<Game>("games").Select(x => x.Id);
            _gameDefaults = _gameDefaults.Where(x => !games.Contains(x.Id)).ToList();

            _gameSeachers = new List<GameSearcher>();
            _games = new List<Game>();
        }

        public AvailableGamesWindow(Mutex m, DatabaseContext_Main db, Service_JsonParser jsp,
            NamedPipeManager npm, List<DatabaseContext_Profile> profiles, ProfilesManager profMng)
            : this(db, ConfigurationPurpose.FirstTime)
        {
            _mutex = m;
            _jsonParser = jsp;
            _namedPipeManager = npm;
            _dbProfiles = profiles;
            _profilesManager = profMng;
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
                   x.RestartSearch();
               });
            }
        }

        [STAThread]
        void InitGames(List<Game> _games)
        {
            GameConfigurationWindow win = new GameConfigurationWindow(_games, ConfigurationPart2);
            win.Show();
        }

        [STAThread]
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
        
        [STAThread]
        private void ConfigurationPart2()
        {
            _db.GetCollection<Game>("games").Delete(x => true);
            _db.GetCollection<GameApplication>("game_apps").Delete(x => true);
            _db.GetCollection<UserProfile>("profiles").Delete(x => true);

            _db.GetCollection<Game>("games").InsertBulk(_games);

            var gameApps = _games.SelectMany(x => x.AutoDetectApplications());

            _db.GetCollection<GameApplication>("game_apps").InsertBulk(gameApps);

            foreach (var item in _games)
            {
                _profilesManager.AddProfile("master", item);
                _dbProfiles.Add(new DatabaseContext_Profile(item.ProfilesDirectory + "\\master", item.Id));
            }

            Defined.Settings.State = StatesOfConfiguration.Ready;

            switch (_purpose)
            {
                case ConfigurationPurpose.FirstTime:
                    MainWindow window = new MainWindow(_mutex, _db, _jsonParser, _namedPipeManager, _dbProfiles, _profilesManager);
                    window.Show();

                    Close();

                    break;
                case ConfigurationPurpose.AddGames:
                    _uponConfigured?.Invoke();
                    break;
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

        private void ConfirmGameDetection(IGameDefault g, Border border)
        {
            border.Dispatcher.BeginInvoke((Action)(() =>
            {
                (_gameDefaults as IList<IGameDefault>).Remove(g);

                if (_gameDefaults.Count() > 0)
                {
                    var parentStkPanel = border.Parent as StackPanel;
                    Border newBorder = new Border()
                    {
                        Style = mainGrid.TryFindResource("GameDetectionBorderStyle") as Style,
                        Child = new Button()
                        {
                            Style = mainGrid.TryFindResource("GameAdditionButtonTemplate") as Style
                        }
                    };
                    if (parentStkPanel.Children.Count < Defined.MAX_GAME_DETECTORS_IN_COLUMN)
                    {
                        parentStkPanel.Children.Add(newBorder);
                    }
                    else
                    {
                        StackPanel newStkPanel = new StackPanel()
                        {
                            Style = mainGrid.TryFindResource("GameDetectorsStackPanelStyle") as Style
                        };
                        newStkPanel.Children.Add(newBorder);
                        gameDetectorsParent.Children.Add(newStkPanel);
                    }
                }
                EMM_GameDetector currDetectorControl = new EMM_GameDetector(g);

                _gameSeachers.Add(new GameSearcher(g, currDetectorControl, _games));
                currDetectorControl.Searcher = _gameSeachers.LastOrDefault();
                currDetectorControl.Searcher.StartSearch();
                border.Child = currDetectorControl;
                border.Background = Defined.Colors.AlternateBackground;

                if (_gameSeachers.Count == 1) thumbnail.Visibility = Visibility.Visible;

            }), DispatcherPriority.Background);
        }
    }
}
