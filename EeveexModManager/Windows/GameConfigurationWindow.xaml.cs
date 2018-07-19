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
using System.Windows.Shapes;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;

using EeveexModManager.Classes;
using EeveexModManager.Controls;
using EeveexModManager.Classes.DatabaseClasses;


using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for GameConfigurationWindow.xaml
    /// </summary>
    public partial class GameConfigurationWindow : Window
    {
        public Game AssociatedGame;
        public string GameName { get; set; }
        private char DriveChar;
        private Action WhenFinishes;


        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public GameConfigurationWindow(Game g, Action whenFinishes)
        {
            AssociatedGame = g;
            DriveChar = g.DataPath[0];
            WhenFinishes = whenFinishes;


            InitializeComponent();
            Title = g.Name + " Directory Configuration";

            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU); //no close button, bitch

            GameImage.Source = Assistant.LoadImageFromResources("Icon - " + AssociatedGame.Id.ToString() + ".png");

            ProfilesDirectory_TB.Text = g.ProfilesDirectory;
            ModsDirectory_TB.Text = g.ModsDirectory;
            DownloadsDirectory_TB.Text = g.DownloadsDirectory;
            BackupsDirectory_TB.Text = g.BackupsDirectory;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesDirectory_TB.Text.Length > 0 && ModsDirectory_TB.Text.Length > 0
                && DownloadsDirectory_TB.Text.Length > 0)
            {
                if (ModsDirectory_TB.Text[0] == DriveChar)
                {
                    if (BackupsDirectory_TB.Text[0] == DriveChar)
                    {
                        AssociatedGame.SetDirectories(ProfilesDirectory_TB.Text, ModsDirectory_TB.Text,
                            DownloadsDirectory_TB.Text, BackupsDirectory_TB.Text);

                        if (!Directory.Exists(AssociatedGame.ModsDirectory))
                            Directory.CreateDirectory(AssociatedGame.ModsDirectory);
                        if (!Directory.Exists(AssociatedGame.DownloadsDirectory))
                            Directory.CreateDirectory(AssociatedGame.DownloadsDirectory);
                        if (!Directory.Exists(AssociatedGame.ProfilesDirectory))
                            Directory.CreateDirectory(AssociatedGame.ProfilesDirectory);
                        if (!Directory.Exists(AssociatedGame.BackupsDirectory))
                            Directory.CreateDirectory(AssociatedGame.BackupsDirectory);
                        WhenFinishes();
                        Close();
                    }
                    else
                        MessageBox.Show("Error! The Backups directory must be in the same drive as the game!");
                }
                else
                    MessageBox.Show("Error! The Mods directory must be in the same drive as the game!");
            }
            else
                MessageBox.Show("Error! Some of the Text Boxes are empty!");
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }

        private void PickDownloadsDir_Click(object sender, RoutedEventArgs e)
        {
            string p = string.Empty;
            FolderBrowserDialog d = new FolderBrowserDialog();
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                p = d.SelectedPath;
                p = p.Length > 3 ? p : p + "EVX\\Downloads\\" + AssociatedGame.Id.ToString();
                ProfilesDirectory_TB.Text = p;
            }
        }

        private void PickProfilesDir_Click(object sender, RoutedEventArgs e)
        {
            string p = string.Empty;
            FolderBrowserDialog d = new FolderBrowserDialog();
            if(d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                p = d.SelectedPath;
                p = p.Length > 3 ? p : p + "EVX\\Profiles\\" + AssociatedGame.Id.ToString();
                ProfilesDirectory_TB.Text = p;
            }
        }

        private void PickModsDir_Click(object sender, RoutedEventArgs e)
        {
            string p = string.Empty;
            FolderBrowserDialog d = new FolderBrowserDialog()
            {
                Description = "The selected directory must be in the same drive as the game."
            };
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                p = d.SelectedPath;
                p = p.Length > 3 ? p : p + "EVX\\Mods\\" + AssociatedGame.Id.ToString();
            }

            while(p[0] != DriveChar)
            {
                MessageBox.Show("Error! The Mods directory must be in the same drive as the game!");
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    p = d.SelectedPath;
                    p = p.Length > 3 ? p : p + "EVX\\Mods\\" + AssociatedGame.Id.ToString();
                }
            }
            ProfilesDirectory_TB.Text = p;
        }

        private void PickBackupsDir_Click(object sender, RoutedEventArgs e)
        {
            string p = string.Empty;
            FolderBrowserDialog d = new FolderBrowserDialog()
            {
                Description = "The selected directory must be in the same drive as the game."
            };
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                p = d.SelectedPath;
                p = p.Length > 3 ? p : p + "EVX\\Backups\\" + AssociatedGame.Id.ToString();
            }

            while (p[0] != DriveChar)
            {
                MessageBox.Show("Error! The Backups directory must be in the same drive as the game!");
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    p = d.SelectedPath;
                    p = p.Length > 3 ? p : p + "EVX\\Backups\\" + AssociatedGame.Id.ToString();
                }
            }
            ProfilesDirectory_TB.Text = p;
        }
    }
}
