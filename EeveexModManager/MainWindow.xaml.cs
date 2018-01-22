using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.IO;

using EeveexModManager.Services;
using EeveexModManager.Classes;
using EeveexModManager.Classes.JsonClasses;
using System.Threading.Tasks;

namespace EeveexModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseContext _db;
        private Service_JsonParser _jsonParser;
        private Json_Config _config;
        private NxmHanlder _nxmHandler;
        private NamedPipeManager _namedPipeManager;

        Game _currGame;

        ModManager modManager;

        List<GameApplication> AvailableApplications;

        Mutex m;
        
        public MainWindow(ref Mutex mutex, ref DatabaseContext db, ref Service_JsonParser jsonParser, ref Json_Config config, ref NamedPipeManager npm)
        {
            InitializeComponent();

            m = mutex;
            _jsonParser = jsonParser;
            _config = config;
            _db = db;
            _currGame = _db.GetCurrentGame();
            _namedPipeManager = npm;

            _nxmHandler = new NxmHanlder(ref config, ref _jsonParser, ref AssociationWithNXM_CheckBox);
            modManager = new ModManager(ref _db, ref ModList_TreeView);
            _namedPipeManager.InitServer(HandlePipeMessage);

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


            AssociationWithNXM_CheckBox.IsChecked = _config.Nxm_Handled;
        }


        ~MainWindow() //removing the symlinks / mod files
        {
            try
            {
                if (_config.Nxm_Handled != AssociationWithNXM_CheckBox.IsChecked)
                {
                    _nxmHandler.AssociationManagement(_config.Nxm_Handled, ref AssociationWithNXM_CheckBox);
                }
            }
            catch (Exception)
            {
            }

        }

        public void HandlePipeMessage(string arg)
        {
            Uri nexusUrl = new Uri(arg);
            modManager.CreateMod(_currGame, new Nexus.NexusUrl(nexusUrl)).GetAwaiter().GetResult();
        }

        public ref TreeView GetModListTree()
        {
            return ref ModList_TreeView;
        }

        List<string> GetAllFilesInDir(string d, List<string> Specifics)
        {
            List<string> files = new List<string>();

            ProcessDirectory(ref files, d, Specifics);

            return files;
        }

        void ProcessDirectory(ref List<string> files, string p, List<string> Specifics)
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
                    ProcessDirectory(ref files, item, Specifics);
                }
            }
        }

        private void LaunchApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            var AvailableApplications = _db.GetCollection<GameApplication>("game_apps").FindAll().Where(x => x.AssociatedGame == _currGame.Id);
            var Application = AvailableApplications.FirstOrDefault(x => x.Index == ApplicationPicker_Selection.SelectedIndex);

            var ActivatedMods = _db.GetCollection<BaseMod>("offline_mods").FindAll().Where(x => x.GameId == _currGame.Id && x.Installed && x.Active).ToList();
            ActivatedMods.AddRange(_db.GetCollection<OnlineMod>("online_mods").FindAll().Where(x => x.GameId == _currGame.Id && x.Installed && x.Active));

            var ActivatedModDirs = ActivatedMods.Select(x => x.ModDirectory).ToList();

            Application.Launch(_currGame, GetAllFilesInDir($@"Mods\{_currGame.Name}", ActivatedModDirs));

        }

        private void Association_Button_Click(object sender, RoutedEventArgs e)
        {
            _nxmHandler.AssociationManagement(_config.Nxm_Handled, ref AssociationWithNXM_CheckBox);
        }
    }
}
