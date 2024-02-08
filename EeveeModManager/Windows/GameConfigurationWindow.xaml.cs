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
        public List<Game> Games;
        private IEnumerable<GamePicker_Control> _gameControls;
        private Game _currGame;
        public string GameName { get; set; }
        private char DriveChar;
        private Action WhenFinishes;


        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public GameConfigurationWindow(List<Game> g, Action whenFinishes)
        {
            Games = g;
            WhenFinishes = whenFinishes;
            InitializeComponent();
            _gameControls = Games.Select(x => new GamePicker_Control(x));
            gamePicker.ItemsSource = _gameControls;
            gamePicker.Items.Refresh();
            gamePicker.SelectedIndex = 0;
        }

        private bool VerifyPaths()
        {
            if (ProfilesDirectory_TB.Text.Length > 0 && ModsDirectory_TB.Text.Length > 0
                && DownloadsDirectory_TB.Text.Length > 0)
            {
                if (ModsDirectory_TB.Text[0] == DriveChar)
                {
                    if (BackupsDirectory_TB.Text[0] == DriveChar)
                    {
                        (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.SetDirectories(ProfilesDirectory_TB.Text, ModsDirectory_TB.Text,
                            DownloadsDirectory_TB.Text, BackupsDirectory_TB.Text);

                        if (!Directory.Exists((gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.ModsDirectory))
                            Directory.CreateDirectory((gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.ModsDirectory);
                        if (!Directory.Exists((gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.DownloadsDirectory))
                            Directory.CreateDirectory((gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.DownloadsDirectory);
                        if (!Directory.Exists((gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.ProfilesDirectory))
                            Directory.CreateDirectory((gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.ProfilesDirectory);
                        if (!Directory.Exists((gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.BackupsDirectory))
                            Directory.CreateDirectory((gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.BackupsDirectory);
                        return true;
                    }
                    else
                        MessageBox.Show("Error! The Backups directory must be in the same drive as the game!");
                }
                else
                    MessageBox.Show("Error! The Mods directory must be in the same drive as the game!");
            }
            else
                MessageBox.Show("Error! Some of the Text Boxes are empty!");
            return false;
        }

        [STAThread]
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if(VerifyPaths())
            {
                WhenFinishes();
                Close();
            }
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
                p = p.Length > 3 ? p : p + "EMM\\Downloads\\" + (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.Id.ToString();
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
                p = p.Length > 3 ? p : p + "EMM\\Profiles\\" + (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.Id.ToString();
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
                p = p.Length > 3 ? p : p + "EMM\\Mods\\" + (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.Id.ToString();
            }

            while(p[0] != DriveChar)
            {
                MessageBox.Show("Error! The Mods directory must be in the same drive as the game!");
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    p = d.SelectedPath;
                    p = p.Length > 3 ? p : p + "EMM\\Mods\\" + (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.Id.ToString();
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
                p = p.Length > 3 ? p : p + "EMM\\Backups\\" + (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.Id.ToString();
            }

            while (p[0] != DriveChar)
            {
                MessageBox.Show("Error! The Backups directory must be in the same drive as the game!");
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    p = d.SelectedPath;
                    p = p.Length > 3 ? p : p + "EMM\\Backups\\" + (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.Id.ToString();
                }
            }
            ProfilesDirectory_TB.Text = p;
        }

        private void gamePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currGame == null || VerifyPaths())
            {
                _currGame = (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame;
                DriveChar = (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.DataPath[0];
                Title = (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.Name + " Directory Configuration";

                var hwnd = new WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU); //no close button, bitch

                GameImage.Source = Assistant.LoadImageFromResources("Icon - " + (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.Id.ToString() + ".png");

                ProfilesDirectory_TB.Text = (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.ProfilesDirectory;
                ModsDirectory_TB.Text = (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.ModsDirectory;
                DownloadsDirectory_TB.Text = (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.DownloadsDirectory;
                BackupsDirectory_TB.Text = (gamePicker.SelectedItem as GamePicker_Control).AssociatedGame.BackupsDirectory;
            }
            else
            {
                gamePicker.SelectedIndex = Games.IndexOf(_currGame);
            }
        }
    }
}
