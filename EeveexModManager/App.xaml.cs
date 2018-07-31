using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Threading;
using System.Diagnostics;

using EeveexModManager.Nexus;
using EeveexModManager.Classes;
using EeveexModManager.Services;
using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        string appGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";
        private Service_JsonParser _jsonParser;
        private DatabaseContext_Main _db;
        private List<DatabaseContext_Profile> _dbProfiles;
        private NamedPipeManager _namedPipeManager;
        private ProfilesManager _profilesManager;
        private Mutex _mutex;

        private string[] RequiredDirectories;

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            _namedPipeManager.CloseConnections();
            _mutex.ReleaseMutex();
            _mutex.Close();
            Defined.Settings.Save();
        }
        
        [STAThread]
        void App_Startup(object sender, StartupEventArgs e)
        {
            NexusUrl modArg = null;
            bool isMainProcess = false;
            _dbProfiles = new List<DatabaseContext_Profile>();

            _mutex = new Mutex(false, "Global\\" + appGuid);
            try
            {
                isMainProcess = _mutex.WaitOne(0, false);
            }
            catch (Exception)
            {
                isMainProcess = false;
            }


            // Application is running
            // Process command line args
            bool startMinimized = false;
            for (int i = 0; i < e.Args.Length; ++i)
            {
                if (e.Args[i] == "/StartMinimized")
                {
                    startMinimized = true;
                }
                else
                {
                    try
                    {
                        modArg = new NexusUrl(new Uri(e.Args[i]));
                    }
                    catch (Exception)
                    {
                        Current.Shutdown();
                    }
                }
            }
            if (!isMainProcess)
            {
                _mutex.ReleaseMutex();
                _mutex.Close();
                if (modArg != null)
                {
                    ProcessStartInfo info = new ProcessStartInfo(Defined.NAMED_PIPE_CLIENT_EXECUTABLE)
                    {
                        Arguments = modArg.ToString(),
                        CreateNoWindow = true
                    };
                    Process.Start(info);
                    Current.Shutdown();
                }
            }
            else
            {
                _namedPipeManager = new NamedPipeManager();
                _jsonParser = new Service_JsonParser();
                RequiredDirectories = new string[] { Defined.Settings.ApplicationDataPath };
                foreach (var item in RequiredDirectories)
                {
                    if (!Directory.Exists(item))
                        Directory.CreateDirectory(item);
                }

                _db = new DatabaseContext_Main();// Strong Type Class
                foreach (var game in _db.GetCollection<Game>("games").FindAll())
                {
                    if (!Directory.Exists(game.ModsDirectory))
                        Directory.CreateDirectory(game.ModsDirectory);
                    if (!Directory.Exists(game.DownloadsDirectory))
                        Directory.CreateDirectory(game.DownloadsDirectory);
                    if (!Directory.Exists(game.ProfilesDirectory))
                        Directory.CreateDirectory(game.ProfilesDirectory);
                }
                _profilesManager = new ProfilesManager(_db);

                foreach (var item in _db.GetCollection<Classes.DatabaseClasses.UserProfile>("profiles").FindAll())
                {
                    _dbProfiles.Add(new DatabaseContext_Profile(item.ProfileDirectory, item.GameId));
                }

                if (Defined.Settings.State == StatesOfConfiguration.FirstTime || _db.GetCollection<Game>("games").FindAll().Count() < 1)
                {
                    _db.FirstTimeSetup(_mutex, _jsonParser, _namedPipeManager, _dbProfiles, _profilesManager);
                    foreach (var item in _dbProfiles)
                    {
                        item.FirstTimeSetup();
                    }
                    return;
                }
                else if(Defined.Settings.State == StatesOfConfiguration.Ready)
                {
                    // Create main application window, starting minimized if specified
                    MainWindow mainWindow = new MainWindow(_mutex, _db, _jsonParser, 
                        _namedPipeManager, _dbProfiles, _profilesManager, modArg);
                    if (startMinimized)
                    {
                        mainWindow.WindowState = WindowState.Minimized;
                    }
                    mainWindow.Show();
                    mainWindow.InitBindings();
                }
            }
        }
    }
}
