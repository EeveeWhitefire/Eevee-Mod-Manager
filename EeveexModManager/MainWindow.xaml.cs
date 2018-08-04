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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Data;

using Newtonsoft.Json;

using EeveexModManager.Services;
using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Controls;
using EeveexModManager.Windows;
using EeveexModManager.Interfaces;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

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
        private NamedPipeManager _namedPipeManager;
        private ProfilesManager _profilesManager;
        private Mutex _mutex;
        private ModManager _modManager;
        private AccountHandler _accountHandler;
        private MultiplicationMathConverter _mulConverter;
        private AdditionMathConverter _addConverter;

        #region Current State Variables
        private Game _currGame { get { return _db.GetCurrentGame(); } }
        private UserProfile _currProfile;
        private DatabaseContext_Profile _currProfileDb;
        #endregion

        public MainWindow(Mutex mutex, DatabaseContext_Main db, Service_JsonParser jsonParser,
            NamedPipeManager npm, List<DatabaseContext_Profile> profiles, ProfilesManager profMngr, Nexus.NexusUrl modUri = null)
        {
            InitializeComponent();

            _mutex = mutex;
            _jsonParser = jsonParser;
            _db = db;
            _dbProfiles = profiles;
            _currProfileDb = _dbProfiles.FirstOrDefault(x => _currGame.Id == x.GameId);
            _namedPipeManager = npm;
            _profilesManager = profMngr;
            _mulConverter = new MultiplicationMathConverter();
            _addConverter = new AdditionMathConverter();
            
            _namedPipeManager.ChangeMessageReceivedHandler(HandlePipeMessage);

            mainGrid.Dispatcher.BeginInvoke((Action)( () =>
            {
                InitGamePicker();
                _accountHandler = new AccountHandler(WhenLogsIn);
                _accountHandler.Init();
                _modManager = new ModManager(_currProfileDb, ModList_View, Downloads_View, _accountHandler);
                profilePicker.SelectionChanged += profilePicker_SelectionChanged;

                if (modUri != null)
                {
                    _modManager.CreateMod(_currGame, modUri).GetAwaiter();
                }
                _db.UpdateCurrentGame(_currGame, SetGame);
            }));

            if (!_namedPipeManager.IsRunning)
            {
                Task.Run(async () => await _namedPipeManager.Listen_NamedPipe(Dispatcher));
            }
        }
        
        private void profilePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ((ComboBox)sender).SelectedIndex;
            if(index == 0)
            {

            }
            else
            {
                _currProfile = _db.GetAll<UserProfile>("profiles")
                    .Where(x => x.GameId == _currGame.Id).ElementAt(index - 1);

                _currProfileDb = _dbProfiles.ElementAt(index - 1);
                ChangeProfile();
            }
        }

        public void InitBindings()
        {
            tabControl2.SetBinding(WidthProperty, new Binding()
            {
                Path = new PropertyPath(ActualWidthProperty),
                Source = mainGrid,
                ConverterParameter = (double)(705.0/1470),
                Converter = _mulConverter
            });
        }

        private void ChangeProfile()
        {
            _modManager.ChangeProfile(_currProfileDb);

            Downloads_View.ItemsSource.GetEnumerator().Reset();
            ModList_View.ItemsSource.GetEnumerator().Reset();
        }
        
        void ConfigureAdditionalGames()
        {
            gamePicker.Dispatcher.BeginInvoke((Action)(() =>
            {
              AvailableGamesWindow gameDetectWindow = new AvailableGamesWindow(_db, uponConfigured: InitGamePicker);
              gameDetectWindow.Show();
            }));
        }
        void AddGameApplication()
        {
            GameApplicationAdderWindow window = new GameApplicationAdderWindow(_db, _currGame, AddAppToSelector);
            window.Show();
        }
        void AddAppToSelector(GameApplication app)
        {
            appPicker.Items.Add(new ApplicationPicker_Control(app));
        }

        void SetGame()
        {
            appPicker?.Items.Clear();

            var apps = _db.GetWhere<GameApplication>("game_apps", (x => x.AssociatedGameId == _currGame.Id));

            appPicker.ItemsSource = apps.Select(x => new ApplicationPicker_Control(x));
            appPicker.Items.Refresh();
            appPicker.SelectedIndex = 0;

            var profiles = _db.GetWhere<UserProfile>("profiles", (x => x.GameId == _currGame.Id));

            foreach (var item in profiles)
            {
                profilePicker.Items.Add(new UserProfile_Control(item, _profilesManager));
            }
            if (!profiles.IsEmpty())
                profilePicker.SelectedIndex = 1;
        }

        #region Login
        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_accountHandler.IsLoggedIn)
            {
                LogInButton.Dispatcher.BeginInvoke((Action)( async () => await _accountHandler.TryLogin()));
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
                LogInButton.Content = "Logout";
               LoginState_TextBox.Text = $"Logged in! Welcome {username}!";
               LoginState_TextBox.Foreground = Brushes.LightGreen;
            });
        }
        private void WhenLogsOut()
        {
            ModList_View.Dispatcher.Invoke(() =>
            {
                _accountHandler.TryLogout();
                (LogInButton.Content as Image).Source = Assistant.LoadImageFromResources($"loginbutton_{_accountHandler.IsLoggedIn}.png");
                LoginState_TextBox.Text = "Not logged in.";
                LoginState_TextBox.Foreground = Brushes.OrangeRed;
                if (File.Exists(Defined.Settings.ApplicationDataPath + "\\token"))
                    File.Delete(Defined.Settings.ApplicationDataPath + "\\token");
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
                Dispatcher.Invoke(() => correctedIndex = gamePicker.SelectedIndex);
                if (nexusUrl.GameName != _currGame.Name_Nexus)
                {
                    var games = _db.GetCollection<Game>("games").FindAll().ToList();
                    correctedIndex = games.FindIndex(x => x.Name_Nexus == nexusUrl.GameName);
                }

                if (correctedIndex >= 0)
                {
                    Task.Run(() =>
                    {
                       Dispatcher.Invoke((Action)(async () =>
                       {
                           gamePicker.SelectedIndex = correctedIndex;
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
            var Application = (appPicker.SelectedItem as ApplicationPicker_Control).App;

            var ActivatedMods = _currProfileDb.GetCollection<Mod>("mods").FindAll()
                .Where(x => x.GameId == _currGame.Id && x.Installed && x.Active).Select( x => x).ToList();

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

        void InitGamePicker()
        {
            gamePicker.Items.Clear();
            _db.GetCollection<Game>("games").Delete(x => !File.Exists(x.ExecutablePath));

            foreach (var game in _db.GetAll<Game>("games"))
            {
                GamePicker_Control c = new GamePicker_Control(game);
                gamePicker.Items.Add(c);
            }
            gamePicker.Items.Add(new TextBlock()
            {
                Text = "<Edit games list>",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center

            });
            gamePicker.Items.Add(new TextBlock()
            {
                Text = "<Configure more games>",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center

            });
            gamePicker.SelectedIndex = _db.FindIndex<Game>("games", (x => x.IsCurrent));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow(_db, _jsonParser);
            window.Show();
        }

        #region Mod List View Event Handlers
        #region Mod Right Click Menu Events

        private void ModList_View_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            FrameworkElement fe = e.Source as FrameworkElement;
            if (ModList_View.SelectedItems.Count < 1)
                fe.ContextMenu = null;
        }

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

        private void Mod_ShowNexus_Click(object sender, RoutedEventArgs e)
        {
            if (ModList_View.SelectedItems.Count == 1 && e.Source == ModList_View)
            {
                var control = ModList_View.SelectedItem as Mod_Control;
                using (Process p = Process.Start(control.AssociatedMod.GetUrl(_currGame.Name_Nexus)))
                { }
            }
            else if(Downloads_View.SelectedItems.Count == 1 && e.Source == Downloads_View)
            {
                var control = Downloads_View.SelectedItem as ModDownload_Control;
                using (Process p = Process.Start(control.AssociatedMod.GetUrl(_currGame.Name_Nexus)))
                { }
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
            FrameworkElement fe = e.Source as FrameworkElement;
            if (Downloads_View.SelectedItems.Count == 1)
            {
                var dl = Downloads_View.SelectedItem as ModDownload_Control;

                fe.ContextMenu = BuildDownloadContextMenu(dl.AssociatedDownload);
            }
            else
                fe.ContextMenu = null;

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

            MenuItem showNxmMia = new MenuItem()
            {
                Header = "Show in Nexus"
            };
            showNxmMia.Click += Mod_ShowNexus_Click;
            theMenu.Items.Add(showNxmMia);

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

        private void ModsView_FilterTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox.Text.IsEmpty())
            {
                textbox.Foreground = Brushes.Gray;
                ModList_View.BorderBrush = Brushes.LightGray;
                _modManager.ModControls.ForEach(x => x.Visibility = Visibility.Visible);
            }
            else
            {
                textbox.Foreground = Brushes.Black;
                ModList_View.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF218FEE")); ;
                _modManager.ModControls.ForEach(x =>
                {
                    if (x.FileName.ToLower().Contains(textbox.Text.ToLower()) || x.ModName.ToLower().Contains(textbox.Text.ToLower()))
                        x.Visibility = Visibility.Visible;
                    else
                        x.Visibility = Visibility.Hidden;
                });
            }
            ModList_View.Items.Refresh();
        }

        private void ModsView_FilterTxt_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox.Text == Defined.DEFAULT_MODS_VIEW_SEARCHER_MESSAGE)
                textbox.Text = string.Empty;
        }

        private void DownloadsView_FilterTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox.Text.IsEmpty())
            {
                textbox.Foreground = Brushes.Gray;
                Downloads_View.BorderBrush = Brushes.LightGray;
                _modManager.DownloadsManager.DownloadControls.ForEach(x => x.Visibility = Visibility.Visible);
            }
            else
            {
                textbox.Foreground = Brushes.Black;
                Downloads_View.BorderBrush = Brushes.LightSkyBlue;
                _modManager.DownloadsManager.DownloadControls.ForEach(x =>
                {
                    if (x.DownloadName.ToLower().Contains(textbox.Text.ToLower()))
                        x.Visibility = Visibility.Visible;
                    else
                        x.Visibility = Visibility.Hidden;
                });
            }
            Downloads_View.Items.Refresh();
        }

        private void DownloadsView_FilterTxt_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox.Text == Defined.DEFAULT_MODS_VIEW_SEARCHER_MESSAGE)
                textbox.Text = string.Empty;
        }

        private void gamePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            int currGameIndex = _db.FindIndex<Game>("games", (x => x.IsCurrent));
            if (index == gamePicker.Items.Count - 1) //last - add
            {
                ConfigureAdditionalGames();
                (sender as ComboBox).SelectedIndex = currGameIndex;
            }
            else if(index == gamePicker.Items.Count - 2) //second last - settings
            {
                (sender as ComboBox).SelectedIndex = currGameIndex;
            }
            else if((sender as ComboBox).SelectedIndex != currGameIndex)
            {
                _db.UpdateCurrentGame((gamePicker.SelectedItem as GamePicker_Control).AssociatedGame, SetGame);
            }
        }

        private void dirOpener_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            string dir = null;
            switch (index)
            {
                case 0: //"Open Game Directory"
                    dir = _currGame.InstallationPath;
                    break;
                case 1: //"Open Profile Directory"
                    dir = _currProfile.ProfileDirectory;
                    break;
                case 2: //"Open EMM Directory"
                    dir = Defined.Settings.InstallationPath;
                    break;
                case 3: //"Open Mods Directory"
                    dir = _currGame.ModsDirectory;
                    break;
                case 4: //"Open Downloads Directory"
                    dir = _currGame.DownloadsDirectory;
                    break;
                case 5: //"Open EMM APPDATA Directory"
                    dir = Defined.Settings.ApplicationDataPath;
                    break;
                default:
                    break;
            }
            Process.Start(dir);
        }

        private void GridViewColumnHeader_Loaded(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader columnHeader = sender as GridViewColumnHeader;
            if (columnHeader.Template.FindName("HeaderBorder", columnHeader) is Border HeaderBorder)
            {
                HeaderBorder.Background = HeaderBorder.Background;
            }
            if (columnHeader.Template.FindName("HeaderHoverBorder", columnHeader) is Border HeaderHoverBorder)
            {
                HeaderHoverBorder.BorderBrush = HeaderHoverBorder.BorderBrush;
            }
            if (columnHeader.Template.FindName("UpperHighlight", columnHeader) is Rectangle UpperHighlight)
            {
                UpperHighlight.Visibility = UpperHighlight.Visibility;
            }
            if (columnHeader.Template.FindName("PART_HeaderGripper", columnHeader) is Thumb PART_HeaderGripper)
            {
                PART_HeaderGripper.Background = Defined.Colors.LightBlue;
            }
        }

        private void PluginsView_FilterTxt_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void pluginPromote_Btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void pluginDemote_Btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}