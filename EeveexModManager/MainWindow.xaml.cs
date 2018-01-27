using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;

using EeveexModManager.Services;
using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Classes.JsonClasses;
using EeveexModManager.Controls;
using EeveexModManager.Windows;

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
        private Mutex _mutex;

        ModManager modManager;

        Game _currGame;
        List<GameApplication> AvailableApplications;

        public MainWindow(Mutex mutex, DatabaseContext db, Service_JsonParser jsonParser, Json_Config config, NamedPipeManager npm)
        {
            InitializeComponent();

            _mutex = mutex;
            _jsonParser = jsonParser;
            _config = config;
            _db = db;
            _currGame = _db.GetCurrentGame();
            _namedPipeManager = npm;

            _nxmHandler = new NxmHandler(config, _jsonParser, AssociationWithNXM_CheckBox);
            modManager = new ModManager(_db, ModList_TreeView);
            _namedPipeManager.ChangeMessageReceivedHandler(HandlePipeMessage);

            if (!_namedPipeManager.IsRunning)
            {
                ApplicationPicker_Selection.Dispatcher.Invoke(() => _namedPipeManager.InitServer(), System.Windows.Threading.DispatcherPriority.Background);
            }


            _gamePicker = new GamePicker_ComboBox(_db.GetCollection<Db_Game>("games").FindAll(), 50, 20, RerunGameDetection, SetGame)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10, 10, 10, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 350,
                Height = 60
            };
            
            MainGrid.Children.Add(_gamePicker);
            AssociationWithNXM_CheckBox.IsChecked = _config.Nxm_Handled;

            SetGame(_db.GetCollection<Db_Game>("games").FindOne(x => x.IsCurrent).EncapsulateToSource());
        }


        ~MainWindow() //removing the symlinks / mod files
        {
            try
            {
                if (_config.Nxm_Handled != AssociationWithNXM_CheckBox.IsChecked)
                {
                    _nxmHandler.AssociationManagement(_config.Nxm_Handled, AssociationWithNXM_CheckBox);
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

        void SetGame(Game game)
        {
            var games = _db.GetCollection<Db_Game>("games").FindAll().Select(x => x.EncapsulateToSource()).ToList();

            (_gamePicker.Items[games.FindIndex(x => x.Id == _currGame.Id)] as GamePicker_Control).GameToCurrent(false);
            (_gamePicker.Items[games.FindIndex(x => x.Id == game.Id)] as GamePicker_Control).GameToCurrent();
            if(game.Name != _currGame.Name)
            {
                _currGame.ToggleIsCurrentGame();
                game.ToggleIsCurrentGame();
                _db.GetCollection<Db_Game>("games").Update(game.EncapsulateToDb());
                _db.GetCollection<Db_Game>("games").Update(_currGame.EncapsulateToDb());
            }
            _currGame = game;

            ApplicationPicker_Selection.Items.Clear();
            ModList_TreeView.Items.Clear();

            AvailableApplications = _db.GetCollection<GameApplication>("game_apps").FindAll().Where(x => x.AssociatedGame == _currGame.Id).ToList();
            AvailableApplications.ForEach(x =>
            {
                StackPanel stkPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };

                TextBlock txtBlock = new TextBlock()
                {
                    Text = x.Name,
                    VerticalAlignment = VerticalAlignment.Center
                };

                stkPanel.Children.Add(txtBlock);
                ApplicationPicker_Selection.Items.Add(stkPanel);
            });

            _db.GetCollection<Db_OnlineMod>("online_mods").FindAll().ToList().ForEach(x =>
            {
                ModList_TreeView.Items.Add(new OnlineMod_Control(x.EncapsulateToSource(), ModList_TreeView.Items.Count, _db));

            });
            _db.GetCollection<Db_BaseMod>("offline_mods").FindAll().ToList().ForEach( x =>
            {
                ModList_TreeView.Items.Add(new Mod_Control(x.EncapsulateToSource(), ModList_TreeView.Items.Count, _db));

            });
        }
        
        public void HandlePipeMessage(string arg)
        {
            Uri uri = new Uri(arg);
            Nexus.NexusUrl nexusUrl = new Nexus.NexusUrl(uri);
            Dispatcher.Invoke( () => modManager.CreateMod(_currGame, new Nexus.NexusUrl(nexusUrl)).GetAwaiter().GetResult(), DispatcherPriority.Background);
        }

        public TreeView GetModListTree()
        {
            return ModList_TreeView;
        }

        List<string> GetAllFilesInDir(string d, List<string> Specifics)
        {
            List<string> files = new List<string>();

            ProcessDirectory(files, d, Specifics);

            return files;
        }

        void ProcessDirectory(List<string> files, string p, List<string> Specifics)
        {
            bool OK = false;
            foreach (var x in Specifics)
            {
                if (p.Contains(x))
                {
                    OK = true;
                    break;
                };
            }

            if (OK || p.EndsWith($@"Mods\{_currGame.Name}"))
            {

                files.AddRange(Directory.GetFiles(p));

                var dirs = Directory.GetDirectories(p);
                foreach (var item in dirs)
                {
                    ProcessDirectory(files, item, Specifics);
                }
            }
        }

        private void LaunchApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            var AvailableApplications = _db.GetCollection<GameApplication>("game_apps").FindAll().Where(x => x.AssociatedGame == _currGame.Id);
            var Application = AvailableApplications.FirstOrDefault(x => x.Index == ApplicationPicker_Selection.SelectedIndex);

            var ActivatedMods = _db.GetCollection<BaseMod>("offline_mods").FindAll().Where(x => x.GameId == _currGame.Id && x.Installed && x.Active).ToList();
            ActivatedMods.AddRange(_db.GetCollection<Db_OnlineMod>("online_mods").Find(x => x.GameId == _currGame.Id && x.Installed && x.Active).Select( x => x.EncapsulateToSource()));

            var ActivatedModDirs = ActivatedMods.Select(x => x.ModDirectory).ToList();

            Application.Launch(_currGame, GetAllFilesInDir($@"Mods\{_currGame.Name}", ActivatedModDirs));

        }

        private void Association_Button_Click(object sender, RoutedEventArgs e)
        {
            _nxmHandler.AssociationManagement(_config.Nxm_Handled, AssociationWithNXM_CheckBox);
        }
    }
}
