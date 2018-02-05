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

        public void Launch(Game game, List<string> modFiles, bool block = false)
        {
            List<string> DirectoriesToDelete = new List<string>();
            List<string> LinksToDelete = new List<string>();
            List<BackupFile> BackupsToRetrieve = new List<BackupFile>();

            foreach (string file in modFiles)
            {
                FileInfo fileInfo = new FileInfo(file);

                var relativePathFile = file.Replace(Directory.GetCurrentDirectory() + $@"\Mods\{game.Name}\", string.Empty);

                string inData = game.DataPath + $@"\{relativePathFile}"; 

                var NeededDirectories = relativePathFile.Split('\\').ToList();

                if (File.Exists(inData))
                {
                    if (!Directory.Exists("Backup Files"))
                    {
                        Directory.CreateDirectory("Backup Files");
                    }

                    string bckup = $@"Backup Files\ev!{fileInfo.Name + fileInfo.Extension}";
                    File.Copy(inData, bckup);
                    BackupsToRetrieve.Add(new BackupFile(inData, bckup));
                    File.Delete(inData);
                }
                else if (NeededDirectories.Count > 1)
                {
                    int count = 1;
                    NeededDirectories.ForEach(y =>
                    {
                        var x = Directory.GetCurrentDirectory() + $@"\{string.Join(@"\", NeededDirectories.Take(count))}";
                        if (!Directory.Exists(x))
                        {
                            Directory.CreateDirectory(x);
                            DirectoriesToDelete.Add(x);
                        }
                    });
                }


                string linkPath = game.DataPath + $@"\{relativePathFile}";

                CreateHardLink(linkPath, file, IntPtr.Zero);
            }

            process.Start();

            if(block)
            {


                process.WaitForExit();
            }

            LinksToDelete.ForEach(x =>
            {
                File.Delete(x);
            });

            BackupsToRetrieve.ForEach(x =>
            {
                x.MoveBack();
            });

            DirectoriesToDelete.ForEach(x =>
           {
               Directory.Delete(x);
           });

        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
    }
}
