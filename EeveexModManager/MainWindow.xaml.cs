﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO.Compression;
using System.IO;
using SevenZip;

namespace EeveexModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string skyrimDir = @"E:\Steam-Main\steamapps\common\Skyrim Special Edition\SkyrimSELauncher.exe";
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void GameLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (GamePicker.SelectedIndex == 0)
            {
                Process skyrimProcess = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = skyrimDir,
                    UseShellExecute = false
                };
                skyrimProcess.StartInfo = startInfo;
                skyrimProcess.Start();
                skyrimProcess.WaitForExit();
                MessageBox.Show("exited");
            }
        }

        void AddModToCategory(int index, Mod mod)
        {
            var x = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var z = new BitmapImage(new Uri("pack://application:,,,/Images/test.png"));

            x.Children.Add(new Image() { Source = z, Width = 20 });
            x.Children.Add(new TextBlock() { Text = $"{mod.Name}        Active:{mod.Active}     Id:{mod.Id}     Version:{mod.Version}" });

            ModelsAndTexturesItem.Items.Add(x);
        }

        private void GameDirButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();

            skyrimDir = dialog.FileName;
        }

        private void InstallModButton_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Archive Files|*.rar;*.zip;*.7z;"
            };

            dialog.ShowDialog();

            ArchiveFile archive = new ArchiveFile(dialog.FileName);
            
            var modProperties = archive.FileName.Split('-');

            if (!Directory.Exists($@"Mods\{archive.FileName}"))
            {
                archive.Extract();

                Mod newMod = new Mod()
                {
                    SourceArchive = archive,
                    Active = false,
                    Id = Convert.ToUInt64(modProperties[1]),
                    Name = modProperties.First(),
                    Installed = true,
                    Version = modProperties.Last()
                };

                AddModToCategory(0, newMod);
            }
        }

        public class ArchiveFile
        {
            public string Extension { get; set; }

            ArchiveType FileType;

            public string FileName { get; set; }
            public string Path { get; set; }

            public ArchiveFile(string fullPath)
            {
                var parts = fullPath.Split('\\');
                var name = parts.Last().Split('.');

                Path = string.Join("\\", parts.Take(parts.Length - 1));
                FileName = name[0];
                Extension = $".{name[1].ToLower()}";

                switch (Extension)
                {
                    case ".rar":
                        FileType = ArchiveType.Rar;
                        break;
                    case ".zip":
                        FileType = ArchiveType.Zip;
                        break;
                    case ".7z":
                        FileType = ArchiveType.SevenZip;
                        break;
                    default:
                        break;
                }
            }

            public void Extract()
            {
                string extractFrom = $@"{Path}\{FileName}{Extension}";
                string extractTo = $@"Mods\{FileName}";

                switch (FileType)
                {
                    case ArchiveType.Rar:
                        break;
                    case ArchiveType.SevenZip:
                        SevenZipBase.SetLibraryPath(@"D:\OneDrive\EeveexModManager\EeveexModManager\7z.dll");
                        SevenZipExtractor extractor = new SevenZipExtractor(extractFrom);
                        extractor.ExtractArchive(extractTo);
                        break;
                    case ArchiveType.Zip:
                        ZipFile.ExtractToDirectory(extractFrom, extractTo);
                        break;
                    default:
                        break;
                }
            }

            enum ArchiveType
            {
                Rar,
                SevenZip,
                Zip
            }

        }


        public class Mod
        {
            public ulong Id { get; set; }
            public string Version { get; set; }
            public string Name { get; set; }
            public bool Active { get; set; }
            public bool Installed { get; set; }

            public ArchiveFile SourceArchive { get; set; }
        }

    }
}
