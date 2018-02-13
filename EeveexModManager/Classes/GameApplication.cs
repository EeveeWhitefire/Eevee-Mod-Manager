using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

using EeveexModManager.Interfaces;
using EeveexModManager.Classes.DatabaseClasses;
using System.Drawing;

namespace EeveexModManager.Classes
{

    public class GameApplication : IGameApplication
    {
        public string Name { get; }
        public string ExecutablePath { get;}
        public GameListEnum AssociatedGameId { get; }
        
        public Process process;

        public GameApplication(string n, string exe, GameListEnum game)
        {
            Name = n;
            ExecutablePath = exe;
            AssociatedGameId = game;

            process = new Process
            {
                StartInfo = new ProcessStartInfo(ExecutablePath)
            };
        }


        public Db_GameApplication EncapsulateToDb()
        {
            return new Db_GameApplication()
            {
                AssociatedGameId = AssociatedGameId,
                ExecutablePath = ExecutablePath,
                Name = Name
            };
        }

        class BackupFile
        {
            public string Source;
            public string OnBackup;

            public BackupFile(string s, string bk)
            {
                Source = s;
                OnBackup = bk;
            }

            public void MoveBack()
            {
                File.Copy(OnBackup, Source);
                File.Delete(OnBackup);
            }
        }

        List<string> GetAllFilesInDir(string d)
        {
            List<string> files = new List<string>();

            ProcessDirectory(ref files, d);

            return files;
        }
        void ProcessDirectory(ref List<string> files, string p)
        {
            files.AddRange(Directory.GetFiles(p));

            var dirs = Directory.GetDirectories(p);
            foreach (var item in dirs)
            {
                ProcessDirectory(ref files, item);
            }
        }

        public void Launch(Game game)
        {
            string mods_full = Directory.GetCurrentDirectory() + '\\' + game.ModsDirectory;
            string prof_full = Directory.GetCurrentDirectory() + '\\' + game.ProfilesDirectory;
            string vfs_full = Directory.GetCurrentDirectory() + "\\launchvfs.py";

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = @"I:\Python37\python.exe",
                Arguments = $"\"{vfs_full}\" \"{ExecutablePath}\" \"{mods_full}\" \"{prof_full}\" \"{game.DataPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using (Process process = Process.Start(start))
            {
            }
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
    }
}
