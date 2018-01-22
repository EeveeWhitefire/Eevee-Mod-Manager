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
using EeveexModManager.Controls;

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for AvailableGamesWindow.xaml
    /// </summary>
    public partial class AvailableGamesWindow : Window
    {
        List<GameSearcher> gameSeachers;
        List<Game> games;

        bool CommencingSearch = false;
        
        public AvailableGamesWindow()
        {
            InitializeComponent();
            StartSearches();
        }

        void StartSearches()
        {
           int counter = 0;
            string[] gameNames = new string[]
                {"TESV : Skyrim Special Edition", "TESV : Skyrim", "Fallout : New Vegas", "Fallout 4", "Fallout 3"};

            gameSeachers = new List<GameSearcher>();
            games = new List<Game>();
            CommencingSearch = true;

            foreach (StackPanel item in GameSearchers_StackPanel.Children)
            {
                foreach (var x in gameNames.Skip(counter))
                {
                    Border border = new Border()
                    {
                        BorderBrush = Brushes.Gainsboro,
                        BorderThickness = new Thickness(2),
                        Margin = new Thickness(0, 0, 0, 5),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    GameDetector_Control currDetectorControl = new GameDetector_Control(gameNames[counter]);

                    gameSeachers.Add(new GameSearcher(gameNames[counter], currDetectorControl));
                    currDetectorControl.Searcher = gameSeachers.LastOrDefault();

                    border.Child = currDetectorControl;

                    item.Children.Add(border);
                    counter++;
                    if (counter >= 3)
                    {
                        break;
                    }
                }
            }
        }

        public IList<Game> GetAvailableGames()
        {
            return games;
        }

        private void RestartScansButton_Click(object sender, RoutedEventArgs e)
        {
            if(CommencingSearch)
            {
                MessageBox.Show("Error! There are still scans commencing! Please wait till each one of them has ended or click on the \"Ignore All\" button!");
            }
            else
            {
                StartSearches();
            }
        }

        private void ConfirmGamesButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameSeachers.Count() > 0)
            {
                gameSeachers.Where(x => x.Confirmed && x.Exists).ToList().ForEach(x =>
                {
                    games.Add(Game.CreateByName(x.Name, x.InstallationPath, x.RegistryName));
                });

                Close();
            }
            else
            {
                MessageBox.Show("Error! No games were found! Please click on the \"Restart Scans\" button to retry!");
                ((Button)sender).IsEnabled = false;
            }
        }

        private void IgnoreAllModsButton_Click(object sender, RoutedEventArgs e)
        {
            gameSeachers.ForEach(x => x.StopSearch());
            gameSeachers.RemoveAll(x => (!x.Search  && !x.Confirmed));
            CommencingSearch = false;
        }
    }
}
