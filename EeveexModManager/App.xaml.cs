using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Threading;
using System.IO.Pipes;
using System.Windows.Threading;

using EeveexModManager.Nexus;
using EeveexModManager.Classes;
using EeveexModManager.Services;
using EeveexModManager.Classes.JsonClasses;
using EeveexModManager.Windows;
using EeveexModManager.Classes.DatabaseClasses;

using Microsoft.EntityFrameworkCore;
using System.Text;
using LiteDB;
using System.Diagnostics;

public enum StatesOfConfiguartion
{
    FirstTime,
    OnPickingCurrentGame,
    Ready
}

namespace EeveexModManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        string appGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";
        private Service_JsonParser _jsonParser;
        private Json_Config _config;
        private DatabaseContext_Main _db;
        private List<DatabaseContext_Profile> _dbProfiles;
        private NamedPipeManager _namedPipeManager;
        private ProfilesManager _profilesManager;

        private string[] RequiredDirectories;

        Mutex mutex;

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            ExitEVX();
        }

        public void ExitEVX()
        {
            try
            {
                _namedPipeManager.CloseConnections();
                mutex.ReleaseMutex();
                mutex.Close();
            }
            catch (Exception)
            {
            }
        }
        
        [STAThread]
        void App_Startup(object sender, StartupEventArgs e)
        {
            NexusUrl modArg = null;
            _dbProfiles = new List<DatabaseContext_Profile>();
            bool isMainProcess = false;

            mutex = new Mutex(false, "Global\\" + appGuid);
            try
            {
                isMainProcess = mutex.WaitOne(0, false);
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
                        ExitEVX();
                    }
                }
            }

            if (!isMainProcess)
            {
                try
                {
                    if (modArg != null)
                    {
                        ProcessStartInfo info = new ProcessStartInfo(Defined.NAMED_PIPE_CLIENT_EXECUTABLE)
                        {
                            Arguments = modArg.ToString(),
                            CreateNoWindow = true
                        };
                    }
                }
                catch (Exception)
                {
                }
                mutex.ReleaseMutex();
                mutex.Close();
            }
            else
            {
                _namedPipeManager = new NamedPipeManager(Defined.NAMED_PIPE_NAME);
                _jsonParser = new Service_JsonParser();

                if (!File.Exists("config.json"))
                {
                    _config = new Json_Config();
                    _jsonParser.UpdateJson(_config);
                }
                else
                {
                    _config = _jsonParser.GetJsonFields<Json_Config>();
                    if (_config.Installation_Path != Directory.GetCurrentDirectory())
                    {
                        _config.Installation_Path = Directory.GetCurrentDirectory();
                        _jsonParser.UpdateJson(_config);
                    }
                }
                RequiredDirectories = new string[] { _config.AppData_Path };
                foreach (var item in RequiredDirectories)
                {
                    if (!Directory.Exists(item))
                        Directory.CreateDirectory(item);
                }

                _db = new DatabaseContext_Main(_config);// Strong Type Class
                foreach (var game in _db.GetCollection<Db_Game>("games").FindAll())
                {
                    if (!Directory.Exists(game.ModsDirectory))
                        Directory.CreateDirectory(game.ModsDirectory);
                    if (!Directory.Exists(game.DownloadsDirectory))
                        Directory.CreateDirectory(game.DownloadsDirectory);
                    if (!Directory.Exists(game.ProfilesDirectory))
                        Directory.CreateDirectory(game.ProfilesDirectory);
                }
                _profilesManager = new ProfilesManager(_db, _config);

                foreach (var item in _db.GetCollection<Db_UserProfile>("profiles").FindAll())
                {
                    _dbProfiles.Add(new DatabaseContext_Profile(item.ProfileDirectory, item.GameId));
                }

                if (_config.State == StatesOfConfiguartion.FirstTime || _db.GetCollection<Db_Game>("games").FindAll().Count() < 1)
                {
                    _db.FirstTimeSetup(mutex, _jsonParser, _config, _namedPipeManager, _dbProfiles, _profilesManager);
                    foreach (var item in _dbProfiles)
                    {
                        item.FirstTimeSetup();
                    }
                    return;
                }
                else if(_config.State == StatesOfConfiguartion.Ready)
                {
                    // Create main application window, starting minimized if specified
                    MainWindow mainWindow = new MainWindow(mutex, _db, _jsonParser, _config, 
                        _namedPipeManager, _dbProfiles, _profilesManager);
                    if (startMinimized)
                    {
                        mainWindow.WindowState = WindowState.Minimized;
                    }
                    mainWindow.Show();
                    mainWindow.InitGUI();
                }
            }
        }
    }
}
