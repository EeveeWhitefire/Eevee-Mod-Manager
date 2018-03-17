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

        private List<BackupFile> _backups;
        private List<string> _directoriesToDelete;
        private List<string> _linksToDelete;
        
        public Process process;

        public GameApplication(string n, string exe, GameListEnum game)
        {
            Name = n;
            ExecutablePath = exe;
            AssociatedGameId = game;
            _backups = new List<BackupFile>();
            _directoriesToDelete = new List<string>();
            _linksToDelete = new List<string>();
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

            public void Backup()
            {
                File.Copy(Source, OnBackup);
                File.Delete(Source);
            }

            public void MoveBack()
            {
                File.Copy(OnBackup, Source);
                File.Delete(OnBackup);
            }
        }

        public void Launch(Game game, List<Mod> mods)
        {
            var filesInData = Assistant.GetAllFilesInDir(game.DataPath);

            var intersecting = filesInData.Select(x => x.Replace(game.DataPath + "\\", string.Empty))
                .Intersect(mods.SelectMany(x => x.FileTree.Select(y => y.RelativePath)));

            foreach (var inter in intersecting)
            {
                _backups.Add(new BackupFile(game.DataPath + "\\" + inter, game.BackupsDirectory + "\\" + inter));
            }

            _backups.ForEach(x => x.Backup());

            mods.SelectMany( x => x.FileTree).ToList().ForEach(x =>
            {
                string linkTo = game.DataPath + "\\" + x.RelativePath;
                string linkFrom = x.FullPath;

                string newDirectory = game.DataPath + "\\" + x.RelativeDirectory;

                if (!Directory.Exists(newDirectory))
                {
                    Directory.CreateDirectory(newDirectory);
                    _directoriesToDelete.Add(newDirectory);
                }

                _linksToDelete.Add(linkTo);

                CreateHardLink(linkTo, linkFrom, IntPtr.Zero);
            });
            

            using (Process process = Process.Start(new ProcessStartInfo(ExecutablePath)))
            {
                process.WaitForExit();

                _linksToDelete.ForEach(x =>
                {
                   File.Delete(x);
                });
                _linksToDelete.Clear();

                _directoriesToDelete.ForEach(x =>
                {
                   Directory.Delete(x);
                });
                _directoriesToDelete.Clear();

                _backups.ForEach(x =>
                {
                   x.MoveBack();
                });
                _backups.Clear();
            }
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
    }
}
