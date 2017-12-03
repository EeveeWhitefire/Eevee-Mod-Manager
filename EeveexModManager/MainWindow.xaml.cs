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
using System.Net.Http;
using SevenZip;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Threading;
using EeveexModManager.Services;
using EeveexModManager.JsonClasses;
using System.Net;
using EeveexModManager.Nexus;

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
        Service_JsonParser jsonParser;
        public static Json_Config config;
        NexusModRepository modRepository;
        IEnvironmentInfo environmentInfo;
        private List<string> m_strFileserverCaptions = new List<string>();

        Mutex m;

        Thickness modListMargin;
        private Dictionary<string, string> tokens;

        public MainWindow(ref Mutex mutex, bool IgnoreLink = false, NexusUrl nexusUrl = null)
        {
            m = mutex;
            modRepository = new NexusModRepository("FalloutNV");
            modRepository.IsOffline = modRepository.Login("kaipowers01", "2001nov06", out tokens);
            modRepository.m_dicAuthenticationTokens = tokens;
            //environmentInfo = new EnvironmentInfo((ISettings)(Properties.Settings.Default));
            if (nexusUrl == null || IgnoreLink)
            {
                InitializeComponent();

                modListMargin = new Thickness(10, 2, 10, 2);
                jsonParser = new Service_JsonParser();

                db = new DatabaseContext();
                db.Database.Migrate();

                config = jsonParser.GetJsonFields<Json_Config>();
                if(IsUrlAssociated("nxm") != config.Nxm_Handled)
                    AssociationManagement(IsUrlAssociated("nxm"));
                AssociationWithNXM_CheckBox.IsChecked = config.Nxm_Handled;

                linksToDelete = new List<string>();

                db.ModList?.ToList().ForEach(mod =>
                {
                    AddModToCategory(mod, db.ModList.ToList().IndexOf(mod));
                });

                if (nexusUrl != null)
                {
                    MessageBox.Show("Installing new mod!");
                    CreateMod(nexusUrl);
                }
            }
        }
        

        ~MainWindow() //removing the symlinks / mod files
        {
            m.Close();

            linksToDelete.ForEach(link =>
            {
                if(File.Exists(link))
                    File.Delete(link);
            });

            try
            {
                if (config.Nxm_Handled != (bool)AssociationWithNXM_CheckBox.IsChecked)
                {
                    AssociationManagement(config.Nxm_Handled);
                }
            }
            catch (Exception)
            {
            }

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

        #region Json_Handling

        #endregion

        #region Mod_Management


        void AddModToCategory(ModDatabase mod, int modIndex)
        {
            int ModNameWidth = 200;

            var x = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            Button newButton = new Button()
            {
                Background = mod.Active ? Brushes.Green : Brushes.Red,
                Content = modIndex,
                Margin = modListMargin,
                Foreground = Brushes.White,
                Width = 15
            };
            newButton.Click += new RoutedEventHandler(ActivateModButton_Click);

            x.Children.Add(newButton);
            x.Children.Add(new TextBlock()
            {
                Text = mod.Name,
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = ModNameWidth
            });
            x.Children.Add(new TextBlock()
            {
                Text = mod.Id.ToString(),
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 50
            });
            x.Children.Add(new TextBlock()
            {
                Text = mod.Version,
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 30
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

            if (dialog.ShowDialog() == true)
            {
                ArchiveFile archive = new ArchiveFile(dialog.FileName);

                var modProperties = archive.FileName.Split('-');

                if (!Directory.Exists($@"Mods\{archive.FileName}{archive.Extension}"))
                {
                    archive.Extract();

                    CreateMod(archive, modProperties);
                }
            }
        }

        public void CreateMod(ArchiveFile archive, string[] props)
        {
            ModDatabase newMod = new ModDatabase
            {
                SourceArchive = archive.GetFullArchivePath(),
                Active = false,
                Id = props[1],
                Name = props.First(),
                Installed = true,
                Version = props.Last()
            };

            AddModToCategory(newMod, db.ModList.Count());

            db.ModList.Add(newMod);
            db.SaveChanges();
        }

        public AddModDescriptor CreateMod(NexusUrl nexusUrl)
        {
            AddModDescriptor amdDescriptor = null;

            if (!db.ModList.Select(x => x.FileId).Contains(nexusUrl.FileId))
            {
                IModFileInfo mfiFile = null;
                List<FileServerInfo> lstFileServerInfo = new List<FileServerInfo>();
                List<Uri> uriFilesToDownload = new List<Uri>();
                try
                {
                    if (String.IsNullOrEmpty(nexusUrl.FileId))
                        mfiFile = modRepository.GetDefaultFileInfo(nexusUrl.ModId);
                    else
                        mfiFile = modRepository.GetFileInfo(nexusUrl.ModId, nexusUrl.FileId);
                    if (mfiFile == null)
                    {
                        MessageBox.Show(String.Format("[{0}] Can't get the file: no file.", nexusUrl.SourceUrl));
                        return null;
                    }
                    lstFileServerInfo = modRepository.GetFilePartInfo(nexusUrl.ModId, mfiFile.Id.ToString(), config.Installation_Path, out string strRepositoryMessage);
                    if (lstFileServerInfo.Count > 0)
                    {
                        foreach (FileServerInfo fsiFileServer in lstFileServerInfo)
                            if (!String.IsNullOrEmpty(fsiFileServer.DownloadLink))
                            {
                                uriFilesToDownload.Add(new Uri(fsiFileServer.DownloadLink));
                                m_strFileserverCaptions.Add(fsiFileServer.Name);
                            }
                    }
                }
                catch (RepositoryUnavailableException e)
                {
                    return null;
                }

                if ((uriFilesToDownload == null) || (uriFilesToDownload.Count <= 0))
                    return null;
                string strSourcePath = System.IO.Path.Combine(@"Downloads\", mfiFile.Filename);
                amdDescriptor = new AddModDescriptor(nexusUrl.SourceUrl, strSourcePath, uriFilesToDownload, TaskStatus.Running, m_strFileserverCaptions);

                /*AddModToCategory(newMod, db.ModList.Count());

                db.ModList.Add(newMod);
                db.SaveChanges(); */

                DownloadMod(amdDescriptor.DownloadFiles);
            }

            return amdDescriptor;
        }

        void DownloadMod(List<Uri> toDownload)
        {
            WebClient webClient = new WebClient();
            int i = 0;
            foreach (var item in toDownload)
            {
                webClient.DownloadFile(item, $@"Downloads\test-{i}.zip");
                i++;
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

        #endregion

        #region NXM_Association
        public static void AssociateNxmFile(string Extension, string KeyName, string OpenWith, string FileDescription)
        {
            string strFileId = Extension + "_File_Type";

            try
            {
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + "." + Extension, null, strFileId);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + strFileId, null, FileDescription, RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + strFileId + @"\DefaultIcon", null, OpenWith + ",0", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + strFileId + @"\shell\open\command", null, "\"" + OpenWith + "\" \"%1\"", RegistryValueKind.String);
            }
            catch (UnauthorizedAccessException)
            {
                throw new InvalidOperationException("Something (usually your antivirus) is preventing the program from interacting with the registry and changing the file associations.");
            }
        }

        protected void UnassociateNxmFile(string name)
        {
            string strFileId =  name + "_File_Type";
            string[] strKeys = Registry.ClassesRoot.GetSubKeyNames();
            if (Array.IndexOf<string>(strKeys, strFileId) != -1)
            {
                Registry.ClassesRoot.DeleteSubKeyTree(strFileId);
                Registry.ClassesRoot.DeleteSubKeyTree("." + name);
            }
        }

        /// <summary>
        /// Determines if the specified URL protocol is associated with the client.
        /// </summary>
        /// <param name="p_strUrlProtocol">The protocol of the URL for which it is to be determined
        /// whether it is associated with the client.</param>
        /// <returns><c>true</c> if the URL protocol is associated with the client;
        /// <c>false</c> otherwise.</returns>
        protected bool IsUrlAssociated(string p_strUrlProtocol)
        {
            string key = Registry.GetValue(@"HKEY_CLASSES_ROOT\" + p_strUrlProtocol, null, null) as string;
            return !String.IsNullOrEmpty(key);
        }

        public void AssociateNxmUrl(string name, string desc, string exePath)
        {
            string strUrlId = "URL:" + desc;
            Registry.SetValue(@"HKEY_CLASSES_ROOT\" + name, null, strUrlId, RegistryValueKind.String);
            Registry.SetValue(@"HKEY_CLASSES_ROOT\" + name, "URL Protocol", "", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_CLASSES_ROOT\" + name + @"\DefaultIcon", null, exePath + ",0", RegistryValueKind.String);
            Registry.SetValue(@"HKEY_CLASSES_ROOT\" + name + @"\shell\open\command", null, "\"" + exePath + "\" \"%1\"", RegistryValueKind.String);
        }
        protected void UnassociateNxmUrl(string name)
        {
            string[] strKeys = Registry.ClassesRoot.GetSubKeyNames();
            if (Array.IndexOf(strKeys, name) != -1)
                Registry.ClassesRoot.DeleteSubKeyTree(name);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        void AssociationManagement(bool isAssociated)
        {
            bool newState = !isAssociated;
            if (!isAssociated)
            {
                //AssociateNxmFile(".nxm", "NXM_File_Handler", Directory.GetCurrentDirectory() + @"\" + "EeveexModManager.exe", "NXM File");
                AssociateNxmUrl("nxm", "Nexus Url", Directory.GetCurrentDirectory() + @"\" + "EeveexModManager.exe");
                AssociationWithNXM_CheckBox.IsChecked = newState;
            }
            else
            {
                //UnassociateNxmFile("nxm");
                UnassociateNxmUrl("nxm");
                AssociationWithNXM_CheckBox.IsChecked = newState;

            }
            config.Nxm_Handled = newState;
            jsonParser.UpdateJson(config);
        }

        private void Association_Button_Click(object sender, RoutedEventArgs e)
        {
            AssociationManagement((bool)AssociationWithNXM_CheckBox.IsChecked);
        }
        #endregion

        private void Test_Download_Click(object sender, RoutedEventArgs e)
        {
            Uri test = new Uri(@"http://filedelivery.nexusmods.com/130/Realistic%20Movement%201.0-64202-.zip?fid=1000044713&ttl=1512245346&ri=8192&rs=8192&setec=65ec2654c8a833a8a2ec32ce12db994e");
            WebClient webClient = new WebClient();
            webClient.DownloadFile(test, @"Downloads\test.zip");
        }
    }
}
