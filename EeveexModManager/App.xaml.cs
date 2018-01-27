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

using EeveexModManager.Nexus;
using EeveexModManager.Classes;
using EeveexModManager.Services;
using EeveexModManager.Classes.JsonClasses;
using EeveexModManager.Windows;
using EeveexModManager.Classes.DatabaseClasses;

using Microsoft.EntityFrameworkCore;
using System.Text;
using LiteDB;

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
        private DatabaseContext _db;
        private NamedPipeManager _namedPipeManager;

        Mutex mutex;

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            ExitEVX();
        }

        public void ExitEVX()
        {
            _namedPipeManager.CloseConnections();
            mutex.ReleaseMutex();
            mutex.Close();
            Environment.Exit(0);
        }

        void Init()
        {
            string[] neededDirs = new string[] { "Mods", "Downloads", "Profiles" , "DLLs"};
            foreach (var dir in neededDirs)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            foreach (var game in _db.GetCollection<Db_Game>("games").FindAll().Select( x => x.EncapsulateToSource()))
            {
                if (!Directory.Exists(game.ModsDirectory))
                    Directory.CreateDirectory(game.ModsDirectory);
            }
        }


        [STAThread]
        void App_Startup(object sender, StartupEventArgs e)
        {
            NexusUrl modArg = null;

            mutex = new Mutex(false, "Global\\" + appGuid);

            _namedPipeManager = new NamedPipeManager("EeveexModManager", !mutex.WaitOne(0, false), this);


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

            if (!mutex.WaitOne(0, false))
            {
                if (modArg != null)
                {
                    _namedPipeManager.SendToServer(modArg.SourceUrl);
                }
                return;
            }


            _jsonParser = new Service_JsonParser();

            if (!File.Exists("config.json"))
            {
                _config = new Json_Config();
                _jsonParser.UpdateJson(_config);
            }
            else
                _config = _jsonParser.GetJsonFields<Json_Config>();


            _db = new DatabaseContext(_config);// Strong Type Class

            Init();

            if (_config.First_Time || _db.GetCollection<Db_Game>("games").Find(x => x.IsCurrent).Count() < 1 || _db.GetCollection<Db_Game>("games").FindAll().Count() < 1)
            {
                _db.FirstTimeSetup(mutex, _jsonParser, _config, _namedPipeManager);
                return;
            }
            else
            {
                // Create main application window, starting minimized if specified
                MainWindow mainWindow = new MainWindow(mutex, _db, _jsonParser, _config, _namedPipeManager);
                if (startMinimized)
                {
                    mainWindow.WindowState = WindowState.Minimized;
                }
                mainWindow.Show();
            }
        }
    }
}
