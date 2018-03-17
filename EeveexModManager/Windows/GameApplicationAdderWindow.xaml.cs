using Microsoft.Win32;
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

using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using System.IO;

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for GameApplicationAdderWindow.xaml
    /// </summary>
    public partial class GameApplicationAdderWindow : Window
    {
        private DatabaseContext_Main _db;
        private Game currGame;
        private Action<GameApplication> OnAppAdded;
        public GameApplicationAdderWindow(DatabaseContext_Main db, Game game, Action<GameApplication> onappadded)
        {
            InitializeComponent();
            _db = db;
            currGame = game;
            OnAppAdded = onappadded;
        }

        private void ChooseExeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Executable Files|*.exe;"
            };

            if(dialog.ShowDialog().Value == true)
            {
                ExecutablePath_TB.Text = dialog.FileName;
                ApplicationName_TB.Text = dialog.SafeFileName;
                DoneButton.IsEnabled = true;
            }
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if(ApplicationName_TB.Text != string.Empty)
            {
                if (File.Exists(ExecutablePath_TB.Text))
                {
                    GameApplication application = new GameApplication(ApplicationName_TB.Text, ExecutablePath_TB.Text, currGame.Id);

                    _db.GetCollection<Db_GameApplication>("game_apps").Insert(application.EncapsulateToDb());
                    OnAppAdded(application);
                    Close();
                }
                else
                    MessageBox.Show($"Error! File \"{ExecutablePath_TB.Text}\" not found!");
            }
            else
                MessageBox.Show($"Error! Application Name cannot be empty!");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
