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
using EeveexModManager.Classes;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO.Compression;
using System.IO;

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

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();

            var filePath = dialog.FileName;
            var fileName = filePath.Split('\\').Last();
            var modProperties = fileName.Replace(".zip", "").Split('-');

            if (!Directory.Exists($@"Mods\{fileName}"))
            {
                ZipFile.ExtractToDirectory(filePath, $@"Mods\{fileName}");

                Mod newMod = new Mod()
                {
                    Active = false,
                    Id = Convert.ToUInt64(modProperties[1]),
                    Name = modProperties.First(),
                    Installed = true,
                    Version = modProperties.Last()
                };

                AddModToCategory(0, newMod);
            }
        }
    }
}
