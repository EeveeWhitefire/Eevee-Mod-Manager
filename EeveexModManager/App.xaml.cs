using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using EeveexModManager.Nexus;
using System.Threading;

namespace EeveexModManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string appGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";

        [STAThread]
        void App_Startup(object sender, StartupEventArgs e)
        {
            Mutex mutex = new Mutex(false, "Global\\" + appGuid);
            NexusUrl modArg = null;

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

            if (!mutex.WaitOne( 0, false ))
            {
                MessageBox.Show("Instance already running");

                if (modArg != null)
                {
                    MessageBox.Show("Starting download!");
                    new MainWindow(ref mutex).CreateMod(modArg);
                }
                if (mutex != null)
                {
                    mutex.Close();
                }
                return;
            }
            // Create main application window, starting minimized if specified
            MainWindow mainWindow = new MainWindow(ref mutex, true, modArg);
            if (startMinimized)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
            mainWindow.Show();
        }
    }
}
