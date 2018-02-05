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
        private List<GameSearcher> gameSeachers;
        private List<Game> games;
        private DatabaseContext _db;
        private Service_JsonParser _jsonParser;
        private Json_Config _config;
        private NamedPipeManager _namedPipeManager;
        private Mutex _mutex;

        private WindowsEnum GetBackTo = WindowsEnum.GameDetectionWindow;

        public AvailableGamesWindow(Mutex m, DatabaseContext db, Service_JsonParser jsp, Json_Config cnfg,
            NamedPipeManager npm, bool getBackButton = false, WindowsEnum backTo = WindowsEnum.MainWindow)
        {
            _mutex = m;
            _db = db;
            _jsonParser = jsp;
            _config = cnfg;
            _namedPipeManager = npm;

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
                    MainWindow mainWindow = new MainWindow(_mutex, _db, _jsonParser, _config, _namedPipeManager);
                    mainWindow.Show();
                    break;
                case WindowsEnum.GamePickerWindow:
                    GamePickerWindow pickingWindow = new GamePickerWindow(_mutex, _db, _jsonParser, _config, _namedPipeManager);
                    pickingWindow.Show();
                    break;
            }
            Close();
        }

        const int GamesPerTab = 5;

        void StartSearches()
        {
           int counter = 0;
            string[] gameNames = new string[]
                {"TESV : Skyrim Special Edition", "TESV : Skyrim", "Fallout : New Vegas",
                    "Fallout 4", "Fallout 3", "Dragon Age II", "Metal Gear Solid V : The Phantom Pain"};

            gameSeachers = new List<GameSearcher>();
            games = new List<Game>();

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

                    GameDetector_Control currDetectorControl = new GameDetector_Control(x);

                    gameSeachers.Add(new GameSearcher(x, currDetectorControl));
                    currDetectorControl.Searcher = gameSeachers.LastOrDefault();
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

        public IList<Game> GetAvailableGames()
        {
            return games;
        }

        private void RestartScansButton_Click(object sender, RoutedEventArgs e)
        {
            if(gameSeachers.Where( x => x.Search).Count() > 0)
            {
                MessageBox.Show("Error! There are still scans commencing! Please wait till each one of them has ended or click on the \"Ignore All\" button!");
            }
            else
            {
                gameSeachers.ForEach(x =>
               {
                   Confirm_Button.Dispatcher.BeginInvoke((Action)(() => x.RestartSearch()), DispatcherPriority.Background);
               });
            }
        }

        void InitGames(List<Game> games)
        {
            games.ForEach(x =>
            {
                if (!Directory.Exists(x.ModsDirectory))
                    Directory.CreateDirectory(x.ModsDirectory);
                if (!Directory.Exists(x.DownloadsDirectory))
                    Directory.CreateDirectory(x.DownloadsDirectory);
            });
        }

        private void ConfirmGamesButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameSeachers.Where( x => x.Confirmed).Count() > 0)
            {
                games = gameSeachers.Where(x => x.Confirmed)
                    .Select(x => Game.CreateByName(x.Name, x.InstallationPath, x.RegistryName)).ToList();
                games.Last().ToggleIsCurrentGame(); //first game is also the current one :)

                InitGames(games);

                _db.GetCollection<Db_Game>("games").Delete(x => true);

                _db.GetCollection<Db_Game>("games").InsertBulk(games.Select( x => x.EncapsulateToDb()));

                _config.State = StatesOfConfiguartion.OnPickingCurrentGame;
                _jsonParser.UpdateJson(_config);

                GamePickerWindow gamePickerWindow = new GamePickerWindow(_mutex, _db, _jsonParser, _config, _namedPipeManager);
                gamePickerWindow.Show();

                Close();
            }
            else
            {
                MessageBox.Show("Error! No games were found! Please click on the \"Restart Scans\" button to retry!");
            }
        }

        private void IgnoreAllModsButton_Click(object sender, RoutedEventArgs e)
        {
            gameSeachers.ForEach(x => x.GuiControl.IgnoreMod());
        }

        private void ConfirmAllGamesButton_Click(object sender, RoutedEventArgs e)
        {
            gameSeachers.Where(x => x.Exists).ToList().ForEach( x =>
            {
                x.GuiControl.ConfirmGame();
            });
        }
    }
}
