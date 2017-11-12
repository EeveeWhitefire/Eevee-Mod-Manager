using System;
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
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Threading;

namespace EeveexModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1,
            Test = 2
        }

        DatabaseContext db;
        string skyrimDir = @"E:\Steam-Main\steamapps\common\Skyrim Special Edition\SkyrimSE.exe";
        List<string> linksToDelete;
        public MainWindow()
        {
            InitializeComponent();
            db = new DatabaseContext();
            db.Database.Migrate();
            linksToDelete = new List<string>();

            db.ModList?.ToList().ForEach(mod =>
            {
                AddModToCategory(mod, db.ModList.ToList().IndexOf(mod));
            });

        }

        ~MainWindow() //removing the symlinks
        {
            linksToDelete.ForEach(link =>
            {
                if(File.Exists(link))
                    File.Delete(link);
            });
        }

        private void GameLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            if (GamePicker.SelectedIndex == 0)
            {
                var mods = db.ModList.Where( x => x.Active && x.Installed).ToList();
                mods.ForEach(mod =>
                {
                    var fileName = mod.SourceArchive.Split('\\').Last();
                    var files = Directory.GetFiles($@"Mods\{fileName}");
                    files.Select( x => x.Split('\\').Last()).ToList().ForEach(file =>
                    {
                        string symbolicLink = $@"E:\Steam-Main\steamapps\common\Skyrim Special Edition\Data\{file}";
                        string filePath = $@"D:\OneDrive\EeveexModManager\EeveexModManager\bin\x64\Debug\Mods\{fileName}\{file}";

                        //CreateSymbolicLink(symbolicLink, filePath, SymbolicLink.Test); //creating symlinks
                        File.Copy(filePath, symbolicLink); //manual copy of mod's files

                        linksToDelete.Add(symbolicLink);

                        //TODO: skyrim needs to be able to access these files (it detects the files but says that the required files are not present)
                    });
                });

                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = skyrimDir,
                    UseShellExecute = true
                };
                process.StartInfo = startInfo;
                process.Start();

                process.WaitForExit();
            }
        }

        void OnItemMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("test");
        }
        void AddModToCategory(ModDatabase mod, int modIndex)
        {
            var x = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            //var z = new BitmapImage(new Uri("pack://application:,,,/Images/test.png"));
            Button newButton = new Button()
            {
                Background = mod.Active ? Brushes.Green : Brushes.Red,
                Content = modIndex,
                Foreground = Brushes.White,
                Width = 15
            };
            newButton.Click += new RoutedEventHandler(ActivateModButton_Click);

            x.Children.Add(newButton);
            x.Children.Add(new TextBlock()
            {
                Text = mod.Name,
                VerticalAlignment = VerticalAlignment.Center
            });
            x.Children.Add(new TextBlock()
            {
                Text = mod.Active.ToString(),
                VerticalAlignment = VerticalAlignment.Center
            });
            x.Children.Add(new TextBlock()
            {
                Text = mod.Id.ToString(),
                VerticalAlignment = VerticalAlignment.Center
            });
            x.Children.Add(new TextBlock()
            {
                Text = mod.Version,
                VerticalAlignment = VerticalAlignment.Center
            });

            ModelsAndTexturesItem.Items.Add(x);
        }

        private void ActivateModButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            if (button.Background == Brushes.Green)
            {
                button.Background = Brushes.Red;
            }
            else
            {
                button.Background = Brushes.Green;
            }

            var mod = db.ModList.ToList()[Convert.ToInt32(button.Content)];
            mod.Active = !mod.Active;
            db.ModList.Update(mod);

            db.SaveChanges();
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

            if (!Directory.Exists($@"Mods\{archive.FileName}{archive.Extension}"))
            {
                archive.Extract();

                CreateMod(archive, modProperties);
            }
        }

        void CreateMod(ArchiveFile archive, string[] props)
        {
            ModDatabase newMod = new ModDatabase
            {
                SourceArchive = archive.GetFullArchivePath(),
                Active = false,
                Id = Convert.ToUInt64(props[1]),
                Name = props.First(),
                Installed = true,
                Version = props.Last()
            };

            AddModToCategory(newMod, db.ModList.Count());

            db.ModList.Add(newMod);
            db.SaveChanges();
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

            public string GetFullArchivePath()
            {
                return $"{Path}\\{FileName}{Extension}";
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

        public interface IMod
        {

        }

        public class Mod : ModDatabase , IMod
        {
        }
    }
}
