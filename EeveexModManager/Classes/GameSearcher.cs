using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

using Microsoft.Win32;

using EeveexModManager.Controls;

namespace EeveexModManager.Classes
{
    public class GameSearcher
    {
        public string Name { get; protected set; }

        public string RegistryName { get; protected set; }
        public string InstallationPath { get; protected set; }
        public string ExecutablePath { get; protected set; }

        TextBox SearchTextBox { get; }
        
        public bool Exists { get; protected set; } = false;
        public bool Confirmed { get; set; } = false;
        public bool Search { get; protected set; } = true;

        public GameSearcher(string n, GameDetector_Control control)
        {
            Name = n;
            SearchTextBox = control.ProgressBar;
            control.Dispatcher.Invoke(() => StartSearch(control), DispatcherPriority.Background);
        }

        public void StopSearch()
        {
            Search = false;
            SearchTextBox.Background = Brushes.Gray;
        }

        public void StartSearch(GameDetector_Control control)
        {
            if (GameIsInstalled())
            {
                Exists = true;
                control.FoundGame();
            }
            StopSearch();
        }

        public bool GameIsInstalled()
        {
            RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (var item in Game.GetRegistryName(Name))
            {

                foreach (var regKey in parentKey.GetSubKeyNames().Select(x => parentKey.OpenSubKey(x))
                    .Where(x => x.GetValue("DisplayName")?.ToString().ToLower().Replace(" ", string.Empty) == 
                    item.ToLower().Replace(" ", string.Empty)))
                {
                    try
                    {
                        InstallationPath = regKey.GetValue("InstallLocation").ToString();
                        RegistryName = item;
                        return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        public static string GetExecutablePath(string n)
        {
            RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths");
            string[] nameList = parentKey.GetSubKeyNames();

            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey regKey = parentKey.OpenSubKey(nameList[i]);
                try
                {
                    if (nameList[i].ToLower() == n.ToLower())
                    {
                        return regKey.GetValue("(Default)").ToString();
                    }
                }
                catch { }
            }
            return string.Empty;
        }
        /*
        void ProcessDirectory(string p)
        {
            if (Search)
            {
                SearchTextBox.Text = p;
                foreach (var x in FilesNeeded)
                {
                    if (p.EndsWith(x))
                    {
                        ExistingFilesCount++;
                        break;
                    };
                }

                if (ExistingFilesCount < FilesNeeded.Count)
                {

                    var dirs = Directory.GetDirectories(p);
                    foreach (var item in dirs)
                    {
                        ProcessDirectory(p);
                    }
                }
            }
        }*/
    }
}
