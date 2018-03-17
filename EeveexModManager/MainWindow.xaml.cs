using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.ComponentModel;

using Newtonsoft.Json;

using EeveexModManager.Services;
using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Classes.JsonClasses;
using EeveexModManager.Controls;
using EeveexModManager.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EeveexModManager.Interfaces;
using System.Diagnostics;

namespace EeveexModManager
{
    public enum WindowsEnum
    {
        MainWindow,
        GameDetectionWindow
    }

    //sorting: 🔼 🔽

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseContext_Main _db;
        private List<DatabaseContext_Profile> _dbProfiles;
        private Service_JsonParser _jsonParser;
        private Json_Config _config;
        private NamedPipeManager _namedPipeManager;
        private ProfilesManager _profilesManager;
        private GamePicker_ComboBox _gamePicker;
        private ApplicationPicker_ComboBox _appPicker;
        private Mutex _mutex;
        private ModManager _modManager;
        private AccountHandler _accountHandler;
        private bool IsLoggedIn = false;

        #region Current State Variables
        private Game _currGame;
        private UserProfile _currProfile;
        private DatabaseContext_Profile _currProfileDb;
        #endregion

        public MainWindow(Mutex mutex, DatabaseContext_Main db, Service_JsonParser jsonParser, 
            Json_Config config, NamedPipeManager npm, List<DatabaseContext_Profile> profiles, 
            ProfilesManager profMngr)
        {
            InitializeComponent();

            _mutex = mutex;
            _jsonParser = jsonParser;
            _config = config;
            _db = db;
            _currGame = _db.GetCurrentGame();
            _dbProfiles = profiles;
            _currProfileDb = _dbProfiles.FirstOrDefault(x => _currGame.Id == x.GameId);
            _namedPipeManager = npm;
            _profilesManager = profMngr;

            _namedPipeManager.ChangeMessageReceivedHandler(HandlePipeMessage);

            _modManager = new ModManager(_currProfileDb, ModList_View, Downloads_View);
            Task.Run( () =>
            {
                _accountHandler = new AccountHandler(_config, WhenLogsIn);
                _modManager.SetAccountHandler(_accountHandler);
            });

            _gamePicker = new GamePicker_ComboBox(_db.GetCollection<Db_Game>("games").FindAll(), 75, 25, RerunGameDetection, SetGame)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 430,
                Height = 80
            };
            ProfileSelector.SelectionChanged += ProfileSelector_SelectionChanged;
            _gamePicker.SelectedIndex = _db.GetCollection<Db_Game>("games").FindAll().ToList().FindIndex(x => x.IsCurrent) + 1;

            RightStack.Children.Add(_appPicker);
            RightStack.Children.Add(_gamePicker);


            if (!_namedPipeManager.IsRunning)
            {
                _namedPipeManager.IsRunning = true;
                Task.Run(() =>
               {
                   while (_namedPipeManager.IsRunning)
                   {
                       _namedPipeManager.Listen_NamedPipe(Dispatcher);
                   }
               });
            }
            GC.Collect();
        }

        private void ProfileSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ((ComboBox)sender).SelectedIndex;
            if(index == 0)
            {

            }
            else
            {
                _currProfile = _db.GetCollection<Db_UserProfile>("profiles").FindAll()
                    .Where(x => x.GameId == _currGame.Id).Select(x => x.EncapsulateToSource()).ElementAt(index - 1);

                _currProfileDb = _dbProfiles.ElementAt(index - 1);
                ChangeProfile();
            }
        }

        private void ChangeProfile()
        {
            _modManager.ChangeProfile(_currProfileDb);

            Downloads_View.ItemsSource.GetEnumerator().Reset();
            ModList_View.ItemsSource.GetEnumerator().Reset();
            GC.Collect();
        }

        ~MainWindow() //removing the symlinks / mod files
        {
            try
            {
            }
            catch (Exception)
            {
            }

        }

        void RerunGameDetection()
        {
            AvailableGamesWindow gameDetectWindow = new AvailableGamesWindow(_mutex, _db, _jsonParser, _config, 
                _namedPipeManager, _dbProfiles, _profilesManager, true, WindowsEnum.MainWindow);
            gameDetectWindow.Show();
            Close();
        }
        void AddGameApplication()
        {
            GameApplicationAdderWindow window = new GameApplicationAdderWindow(_db, _currGame, AddAppToSelector);
            window.Show();
        }
        void AddAppToSelector(GameApplication app)
        {
            _appPicker.Items.Add(new ApplicationPicker_Control(app));
        }

        void SetGame(Game game)
        {
            var games = _db.GetCollection<Db_Game>("games").FindAll().Select(x => x.EncapsulateToSource()).ToList();

            (_gamePicker.Items[games.FindIndex(x => x.Id == _currGame.Id) + 1] as GamePicker_Control).GameToCurrent(false);
            (_gamePicker.Items[games.FindIndex(x => x.Id == game.Id) + 1] as GamePicker_Control).GameToCurrent();
            if(game.Name != _currGame.Name)
            {
                _currGame.ToggleIsCurrentGame();
                if (_currGame.IsCurrent)
                    _currGame.ToggleIsCurrentGame();

                game.ToggleIsCurrentGame();
                if (!game.IsCurrent)
                    game.ToggleIsCurrentGame();
                _db.GetCollection<Db_Game>("games").Update(game.EncapsulateToDb());
                _db.GetCollection<Db_Game>("games").Update(_currGame.EncapsulateToDb());
            }
            _currGame = game;

            _appPicker?.Items.Clear();
            var AvailableApplications = _db.GetCollection<Db_GameApplication>("game_apps").FindAll()
                .Where(x => x.AssociatedGameId == _currGame.Id).ToList();

            if(_appPicker != null)
            {
                _appPicker.Init(AvailableApplications);
            }
            else
                _appPicker = new ApplicationPicker_ComboBox(AvailableApplications, AddGameApplication);

            foreach (var item in _db.GetCollection<Db_UserProfile>("profiles")
                .FindAll().Where(x => x.GameId == _currGame.Id))
            {
                ProfileSelector.Items.Add(new UserProfile_Control(item, _profilesManager));
            }
            ProfileSelector.SelectedIndex = 1;
        }

        #region Login
        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLoggedIn)
            {
                LogInButton.Dispatcher.Invoke(() => _accountHandler.TryToLogIn().GetAwaiter().GetResult());
            }
            else
            {
                WhenLogsOut();
            }
        }
        private void WhenLogsIn(string username)
        {
            ModList_View.Dispatcher.Invoke(() =>
            {
               IsLoggedIn = true;
               LogInButton.Content = "Logout";
               LoginState_TextBox.Text = $"Logged in! Welcome {username}!";
               LoginState_TextBox.Foreground = Brushes.Green;
            });
        }
        private void WhenLogsOut()
        {
            ModList_View.Dispatcher.Invoke(() =>
            {
                IsLoggedIn = false;
                LogInButton.Content = "Login";
                LoginState_TextBox.Text = "Not logged in.";
                LoginState_TextBox.Foreground = Brushes.Red;
                if (File.Exists(_config.AppData_Path + "\\token"))
                    File.Delete(_config.AppData_Path + "\\token");
            });
        }
        #endregion

        public void HandlePipeMessage(string arg)
        {
            if(arg != string.Empty)
            {
                Uri uri = new Uri(arg);
                Nexus.NexusUrl nexusUrl = new Nexus.NexusUrl(uri);
                int correctedIndex = -1;
                Dispatcher.Invoke(() => correctedIndex = _gamePicker.SelectedIndex);
                if (nexusUrl.GameName != _currGame.Name_Nexus)
                {
                    var games = _db.GetCollection<Db_Game>("games").FindAll().Select(x => x.EncapsulateToSource()).ToList();
                    correctedIndex = games.FindIndex(x => x.Name_Nexus == nexusUrl.GameName);
                }

                if (correctedIndex >= 0)
                {
                    Task.Run(() =>
                    {
                       Dispatcher.Invoke((Action)(async () =>
                       {
                           _gamePicker.SelectedIndex = correctedIndex;
                           await _modManager.CreateMod(_currGame, nexusUrl);
                       }), DispatcherPriority.ApplicationIdle);
                    });
                }
                else
                {
                    MessageBox.Show("Error: You are trying to install a mod for a game you don't have on your machine or isn't configured");
                }
            }


            //Thread t = new Thread(_modManager.CreateMod);
            //t.SetApartmentState(ApartmentState.STA);
            //t.Start();
        }

        private void LaunchApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            var Application = (_appPicker.SelectedItem as ApplicationPicker_Control).App;

            var ActivatedMods = _currProfileDb.GetCollection<Db_Mod>("mods").FindAll()
                .Where(x => x.GameId == _currGame.Id && x.Installed && x.Active).Select( x => x.EncapsulateToSource()).ToList();

            using (StreamWriter x = new StreamWriter($"{_currProfile.ProfileDirectory}\\modlist.txt"))
            {
                ActivatedMods.ForEach(m =>
                {
                    x.WriteLine($"1 {m.Name}");
                });
                x.Close();
            }

            Application.Launch(_currGame, ActivatedMods);

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow(_config, _db, _jsonParser);
            window.Show();
        }

        #region Mod List View Event Handlers

        private void ModList_View_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ModList_View.SelectedItems.Count == 1) //double clicked on only one download
            {
                var mControl = ModList_View.SelectedItem as Mod_Control;
                mControl.AssociatedMod.ToggleIsActive();
                mControl.UpdateProperties();
                ModList_View.Items.Refresh();
                _modManager.UpdateMod(mControl.AssociatedMod);
            }
        }

        #region Mod Right Click Menu Events

        private void Mod_OpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (ModList_View.SelectedItems.Count == 1)
            {
                var control = ModList_View.SelectedItem as Mod_Control;

                using (Process p = Process.Start(new ProcessStartInfo(control.AssociatedMod.ModDirectory)))
                {

                };
            }
        }

        private void Mod_UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModList_View.SelectedItems.Count == 1)
            {
                var control = ModList_View.SelectedItem as Mod_Control;

                _modManager.RemoveMod(control.AssociatedMod.FileId, ModList_View.SelectedIndex);
            }
        }
        #endregion

        #endregion

        #region Download List View Event Handlers

        private void Downloads_View_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Downloads_View.SelectedItems.Count == 1) //double clicked on only one download
            {
                var dlControl = Downloads_View.SelectedItem as ModDownload_Control;
                if (dlControl.AssociatedDownload.DownloadState == DownloadStates.Finished)
                    dlControl.InstallMod();
            }
        }

        private void Downloads_View_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (Downloads_View.SelectedItems.Count == 1)
            {
                var dl = Downloads_View.SelectedItem as ModDownload_Control;

                FrameworkElement fe = e.Source as FrameworkElement;
                fe.ContextMenu = BuildDownloadContextMenu(dl.AssociatedDownload);
            }

        }

        private ContextMenu BuildDownloadContextMenu(Download dl)
        {
            ContextMenu theMenu = new ContextMenu();

            if(dl.DownloadState == DownloadStates.InProgress || dl.DownloadState == DownloadStates.Paused)
            {
                MenuItem mia = new MenuItem()
                {
                    Header = dl.DownloadState == DownloadStates.InProgress ? "Pause" : "Resume"
                };
                mia.Click += Downloads_View_PauseOrResumeHandler;
                theMenu.Items.Add(mia);
            }
            if(dl.DownloadState == DownloadStates.Canceled || dl.DownloadState == DownloadStates.InProgress)
            {
                MenuItem mia = new MenuItem()
                {
                    Header = dl.DownloadState == DownloadStates.Canceled ? "Restart" : "Cancel",
                };
                mia.Click += Downloads_View_RestartOrCancelHandler;
                theMenu.Items.Add(mia);
            }
            return theMenu.Items.Count > 0 ? theMenu : null;
        }

        private void Downloads_View_RestartOrCancelHandler(object sender, RoutedEventArgs e)
        {
            var dlControl = Downloads_View.SelectedItem as ModDownload_Control;
            if (dlControl.AssociatedDownload.DownloadState == DownloadStates.InProgress)
                dlControl.CancelDownload();
            else if (dlControl.AssociatedDownload.DownloadState == DownloadStates.Canceled)
                dlControl.RestartDownload();
        }

        private void Downloads_View_PauseOrResumeHandler(object sender, RoutedEventArgs e)
        {
            var dlControl = Downloads_View.SelectedItem as ModDownload_Control;
            if (dlControl.AssociatedDownload.DownloadState == DownloadStates.InProgress)
                dlControl.PauseDownload();
            else if (dlControl.AssociatedDownload.DownloadState == DownloadStates.Paused)
                dlControl.ResumeDownload();
        }
        #endregion
    }
}
