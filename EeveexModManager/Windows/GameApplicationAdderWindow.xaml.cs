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

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for GameApplicationAdderWindow.xaml
    /// </summary>
    public partial class GameApplicationAdderWindow : Window
    {
        private DatabaseContext _db;
        private Game currGame;
        private Action<GameApplication> OnAppAdded;
        string exePath;
        public GameApplicationAdderWindow(DatabaseContext db, Game game, Action<GameApplication> onappadded)
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
                exePath = dialog.FileName;
                ApplicationNameTBox.Text = dialog.SafeFileName;
                DoneButton.IsEnabled = true;
            }
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if(ApplicationNameTBox.Text != string.Empty)
            {
                GameApplication application = new GameApplication(ApplicationNameTBox.Text + ".exe", exePath, currGame.Id);

                _db.GetCollection<Db_GameApplication>("game_apps").Insert(application.EncapsulateToDb());
                OnAppAdded(application);
                Close();

            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
