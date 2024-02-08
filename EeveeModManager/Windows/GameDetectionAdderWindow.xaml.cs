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

using EeveexModManager.Controls;
using EeveexModManager.Classes;
using EeveexModManager.Interfaces;

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for GameDetectionAdderWindow.xaml
    /// </summary>
    public partial class GameDetectionAdderWindow : Window
    {
        private IEnumerable<IGameDefault> _gameDefaults;
        private Border _border;
        private Action<IGameDefault, Border> _callback;

        public GameDetectionAdderWindow(IEnumerable<IGameDefault> availableGames, 
            Border border, Action<IGameDefault, Border> callback)
        {
            InitializeComponent();

            _gameDefaults = availableGames;
            _callback = callback;
            _border = border;

            foreach (var g in availableGames)
            {
                A panel = new A(g);
                availableGames_ComboBox.Items.Add(panel);
            }
            availableGames_ComboBox.SelectedIndex = 0;
        }

        private void ConfirmGameButton_Click(object sender, RoutedEventArgs e)
        {
            _callback((availableGames_ComboBox.SelectedItem as A).GameDefault, _border);
            Close();
        }

        private void CancelGameButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private class A : StackPanel
        {
            public IGameDefault GameDefault { get; protected set; }

            public A(IGameDefault g)
            {
                Orientation = Orientation.Horizontal;
                Children.Add(new Image()
                {
                    Source = Assistant.LoadGameImage(g.Id.ToString()),
                    VerticalAlignment = VerticalAlignment.Center,
                    Height = 75,
                    Width = 75,
                    Margin = new Thickness(0,0,10,0)
                });
                Children.Add(new TextBlock()
                {
                    Text = g.Id.ToString(),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 30
                });
                GameDefault = g;
            }
        }
    }
}
