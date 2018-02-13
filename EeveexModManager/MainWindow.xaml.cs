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

namespace EeveexModManager
{
    public enum WindowsEnum
    {
        MainWindow,
        GameDetectionWindow,
        GamePickerWindow
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseContext _db;
        private Service_JsonParser _jsonParser;
        private Json_Config _config;
        private NxmHandler _nxmHandler;
        private NamedPipeManager _namedPipeManager;
        private GamePicker_ComboBox _gamePicker;
        private ApplicationPicker_ComboBox _appPicker;
        private Mutex _mutex;
        private Json_AccountInfo User;
        private bool IsLoggedIn = false;

        ModManager modManager;

        Game _currGame;

        public MainWindow(Mutex mutex, DatabaseContext db, Service_JsonParser jsonParser, Json_Config config, NamedPipeManager npm)
        {
            InitializeComponent();

            _mutex = mutex;
            _jsonParser = jsonParser;
            _config = config;
            _db = db;
            _currGame = _db.GetCurrentGame();
            _namedPipeManager = npm;

            _nxmHandler = new NxmHandler(config, _jsonParser, AssociationWithNXM_CheckBox, _db);
            modManager = new ModManager(_db, ModList_TreeView, DownloadsTreeView);
            _namedPipeManager.ChangeMessageReceivedHandler(HandlePipeMessage);

            if(File.Exists("UserCredentials"))
            {
                try
                {
                    string raw = Cryptographer.Decrypt(File.ReadAllText("UserCredentials"));
                    User = JsonConvert.DeserializeObject<Json_AccountInfo>(raw);

                    AccountLoginWindow.TryToLogIn(User.Username, User.Password, WhenLogsIn);
                }
                catch (Exception)
                {
                }
            }

            _gamePicker = new GamePicker_ComboBox(_db.GetCollection<Db_Game>("games").FindAll(), 50, 20, RerunGameDetection, SetGame)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 400,
                Height = 60
            };
            SetGame(_db.GetCollection<Db_Game>("games").FindOne(x => x.IsCurrent).EncapsulateToSource());
            RightStack.Children.Add(_appPicker);
            RightStack.Children.Add(_gamePicker);
            AssociationWithNXM_CheckBox.IsChecked = _config.Nxm_Handled;

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
        }


        ~MainWindow() //removing the symlinks / mod files
        {
            try
            {
                if (_config.Nxm_Handled != AssociationWithNXM_CheckBox.IsChecked)
                {
                    _nxmHandler.AssociationManagement(_config.Nxm_Handled, AssociationWithNXM_CheckBox, _db.GetCollection<Db_Game>("games").FindAll());
                }
            }
            catch (Exception)
            {
            }

        }

        void RerunGameDetection()
        {
            AvailableGamesWindow gameDetectWindow = new AvailableGamesWindow(_mutex, _db, _jsonParser, _config, _namedPipeManager, true, WindowsEnum.MainWindow);
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

            (_gamePicker.Items[games.FindIndex(x => x.Id == _currGame.Id)] as GamePicker_Control).GameToCurrent(false);
            (_gamePicker.Items[games.FindIndex(x => x.Id == game.Id)] as GamePicker_Control).GameToCurrent();
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

            ModList_TreeView.Items.Clear();


            _db.GetCollection<Db_Mod>("mods").FindAll().Where( x => x.GameId == _currGame.Id).ToList().ForEach(x =>
            {
                ModList_TreeView.Items.Add(new Mod_Control(x.EncapsulateToSource(), _db));

            });
        }
        
        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            if(!IsLoggedIn)
            {
                AccountLoginWindow loginWindow = new AccountLoginWindow(WhenLogsIn);
                loginWindow.Show();
            }
            else
            {
                WhenLogsOut();
            }
        }

        private void WhenLogsIn(string CookieSid, Json_AccountInfo user)
        {
            User = user;
            modManager.UpdateCookieSid(CookieSid);
            IsLoggedIn = true;
            LogInButton.Content = "Logout";
            LoginState_TextBox.Text = $"Logged in, welcome {user.Username}!";
            LoginState_TextBox.Foreground = Brushes.Green;
        }

        private void WhenLogsOut()
        {
            modManager.ToggleCanAccessApi();
            IsLoggedIn = false;
            LogInButton.Content = "Login";
            LoginState_TextBox.Text = "Not logged in.";
            LoginState_TextBox.Foreground = Brushes.Red;
        }
        
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

                    Dispatcher.Invoke((Action)(async () =>
                    {
                        _gamePicker.SelectedIndex = correctedIndex;
                        await modManager.CreateMod(_currGame, nexusUrl);
                    }), DispatcherPriority.ApplicationIdle);
                }
                else
                {
                    MessageBox.Show("Error: You are trying to install a mod for a game you don't have on your machine or isn't configured");
                }
            }


            //Thread t = new Thread(modManager.CreateMod);
            //t.SetApartmentState(ApartmentState.STA);
            //t.Start();
        }

        public TreeView GetModListTree()
        {
            return ModList_TreeView;
        }

        private void LaunchApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            var Application = (_appPicker.SelectedItem as ApplicationPicker_Control).App;

            var ActivatedMods = _db.GetCollection<Db_Mod>("mods").FindAll()
                .Where(x => x.GameId == _currGame.Id && x.Installed && x.Active).ToList();

            var ActivatedModDirs = ActivatedMods.Select(x => x.ModDirectory).ToList();

            using (StreamWriter x = new StreamWriter($"{_currGame.ProfilesDirectory}\\modlist.txt"))
            {
                ActivatedMods.ForEach(m =>
                {
                    var dd = new DirectoryInfo(m.ModDirectory);
                    x.WriteLine($"1 {dd.Name}");
                });
                x.Close();
            }

            Application.Launch(_currGame);

        }

        private void Association_Button_Click(object sender, RoutedEventArgs e)
        {
            _nxmHandler.AssociationManagement(_config.Nxm_Handled, AssociationWithNXM_CheckBox, _db.GetCollection<Db_Game>("games").FindAll());
        }
    }
}
