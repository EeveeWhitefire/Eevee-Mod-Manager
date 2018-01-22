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
        ~App()
        {
            mutex.Close();
        }



    [STAThread]
        void App_Startup(object sender, StartupEventArgs e)
        {
            NexusUrl modArg = null;

            mutex = new Mutex(false, "Global\\" + appGuid);
            _namedPipeManager = new NamedPipeManager("EeveexModManager");

             _db = new DatabaseContext();// Strong Type Class

            _jsonParser = new Service_JsonParser();
            _config = _jsonParser.GetJsonFields<Json_Config>();

            if (_config.First_Time)
            {
                AvailableGamesWindow window = new AvailableGamesWindow();
                window.Show();
            }

            if (!_config.First_Time)
            {
                // Application is running
                // Process command line args
                bool startMinimized = false;
                for (int i = 0; i != e.Args.Length; ++i)
                {
                    if (e.Args[i] == "/StartMinimized")
                    {
                        startMinimized = true;
                    }
                    else
                    {
                        MessageBox.Show(e.Args[i]);
                        modArg = new NexusUrl(new Uri(e.Args[i]));
                    }
                }

                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Instance already running");

                    if (modArg != null)
                    {
                        _namedPipeManager.SendToServer(modArg.SourceUrl);
                    }
                    return;
                }

                // Create main application window, starting minimized if specified
                MainWindow mainWindow = new MainWindow(ref mutex, ref _db, ref _jsonParser, ref _config, ref _namedPipeManager);
                if (startMinimized)
                {
                    mainWindow.WindowState = WindowState.Minimized;
                }
                mainWindow.Show();
            }
        }
    }
}
