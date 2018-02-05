using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using EeveexModManager.Classes;
using EeveexModManager.Interfaces;
using EeveexModManager.Define;
using System.Windows.Media;

namespace EeveexModManager.Controls
{
    public class GameDetector_Control : StackPanel
    {

        public string GameName { get; protected set; }
        public GameSearcher Searcher { get; set; }

        public ConfirmGame_Button AuthorizeButton { get; protected set; }
        public ShapedButton<Rectangle> CancelButton { get; protected set; }
        public TextBox ProgressBar { get; protected set; }

        public StackPanel UpperPanel { get; protected set; }
        public StackPanel ButtonsPanel { get; protected set; }

        public GameDetector_Control(string gameN) : base()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Horizontal;
            GameName = gameN;

            InitializeGui();
        }

        void InitializeGui()
        {
            UpperPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 10, 0),
                Width = 350
            };
            
            UpperPanel.Children.Add(new TextBlock()
            {
                Text = GameName,
                FontSize = 20,
                Margin = new Thickness(0,0,0,10)
            });

            ProgressBar = new TextBox()
            {
                IsReadOnly = true,
                IsEnabled = false,
                Height = 25,
                Margin = new Thickness(0, 0, 0, 10)
            };


            UpperPanel.Children.Add(ProgressBar);

            ButtonsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            AuthorizeButton = new ConfirmGame_Button(GameName, Defined.MODPICKINGBUTTON_SIZE, Defined.MODPICKINGBUTTON_SIZE);

            AuthorizeButton.Click += new RoutedEventHandler(ConfirmGameButton_Click);

            CancelButton = new SquareButton(Defined.MODPICKINGBUTTON_SIZE, Defined.MODPICKINGBUTTON_SIZE, "Button_RedX")
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10,0,0,10)
            };
            CancelButton.Click += new RoutedEventHandler(IgnoreModButton_Click);


            ButtonsPanel.Children.Add(CancelButton);
            ButtonsPanel.Children.Add(AuthorizeButton);

            ResetGUI();

            UpperPanel.Children.Add(ButtonsPanel);
            
            Image icon = new Image()
            {
                Width = 140,
                Source = new BitmapImage(new Uri($"pack://application:,,,/EeveexModManager;component/Resources/Icon - {GameName.Replace(" : ", " ")}.png", UriKind.Absolute)),
                Height = 140,
                Margin = new Thickness(0,0,10,0),
                VerticalAlignment = VerticalAlignment.Top
            };
            Children.Add(icon);
            Children.Add(UpperPanel);

        }


        private void IgnoreModButton_Click(object sender, RoutedEventArgs e)
        {
            IgnoreMod();
        }

        public void IgnoreMod()
        {
            if (Searcher.Search || Searcher.Exists)
            {
                Searcher.Search = false;
                Searcher.Confirmed = false;

                CancelButton.State_ToDisabled();
                AuthorizeButton.State_ToDisabled();
                if (ButtonsPanel.Children[ButtonsPanel.Children.Count - 1].GetType() == typeof(TextBlock))
                {
                    ButtonsPanel.Children.RemoveAt(ButtonsPanel.Children.Count - 1);
                }
                ButtonsPanel.Children.Add(new TextBlock()
                {
                    Text = "Game Ignored!",
                    Margin = new Thickness(35, 0, 0, 0),
                    Foreground = Brushes.Purple,
                    FontSize = GameSearcher.GameStateTextSize,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
        }

        public void ResetGUI()
        {
            if (ButtonsPanel.Children[ButtonsPanel.Children.Count - 1].GetType() == typeof(TextBlock))
            {
                ButtonsPanel.Children.RemoveAt(ButtonsPanel.Children.Count - 1);
            }

            AuthorizeButton.State_ToDisabled();
            CancelButton.State_ToEnabled();
            AuthorizeButton.Visibility = Visibility.Hidden;
        }

        public void FoundGame()
        {
            AuthorizeButton.State_ToEnabled();
        }

        public void ConfirmGame()
        {
            CancelButton.State_ToDisabled();
            AuthorizeButton.State_ToDisabled();
            if (ButtonsPanel.Children[ButtonsPanel.Children.Count - 1].GetType() == typeof(TextBlock))
            {
                ButtonsPanel.Children.RemoveAt(ButtonsPanel.Children.Count - 1);
            }

            ButtonsPanel.Children.Add(new TextBlock()
            {
                Text = "Game Confirmed!",
                Margin = new Thickness(35, 0, 0, 0),
                Foreground = Brushes.DarkGoldenrod,
                FontSize = GameSearcher.GameStateTextSize,
                VerticalAlignment = VerticalAlignment.Center,
            });

            Searcher.Confirmed = true;
        }

        private void ConfirmGameButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmGame();
        }

    }
}
