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
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Interfaces;

namespace EeveexModManager.Controls
{
    /// <summary>
    /// Interaction logic for EMM_GameDetector.xaml
    /// </summary>
    public partial class EMM_GameDetector : UserControl
    {
        public IGameDefault GameDefault { get; protected set; }
        public GameSearcher Searcher { get; set; }

        public EMM_GameDetector(IGameDefault game)
        {
            InitializeComponent();
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            ResetGUI();
            GameDefault = game;
            gameIcon.Source = Assistant.LoadGameImage(GameDefault.Id.ToString());
            gameName.Text = GameDefault.Name;
        }

        public void ResetGUI()
        {
            status.Visibility = Visibility.Hidden;

            confirmGame_Btn.IsEnabled = false;
            ignoreGame_Btn.IsEnabled = false;
        }

        public void FoundGame()
        {
            confirmGame_Btn.IsEnabled = true;
        }

        public void ConfirmGame()
        {
            confirmGame_Btn.IsEnabled = false;
            ignoreGame_Btn.IsEnabled = false;
            status.Text = "Game Confirmed!";
            status.Foreground = Brushes.Goldenrod;

            Searcher.ConfirmGame();

            if (Searcher.ConfirmedGames.Count == 1)
            {
                Searcher.AssociatedGame.ToggleIsCurrentGame();
            }
        }

        private void ignoreGame_Btn_Click(object sender, RoutedEventArgs e)
        {
            IgnoreMod();
        }

        public void IgnoreMod()
        {
            if (Searcher.Search || Searcher.Exists)
            {
                Searcher.Search = false;

                confirmGame_Btn.IsEnabled = false;
                ignoreGame_Btn.IsEnabled = false;
                status.Text = "Game Ignored!";
                status.Foreground = Brushes.Purple;
            }
        }

        private void confirmGame_Btn_Click(object sender, RoutedEventArgs e)
        {
            ConfirmGame();
        }
    }
}
