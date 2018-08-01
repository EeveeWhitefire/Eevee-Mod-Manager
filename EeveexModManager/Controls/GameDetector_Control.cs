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
using System.Windows.Media;

namespace EeveexModManager.Controls
{
    public class GameDetector_Control : StackPanel
    {
        public IGameDefault GameDefault { get; protected set; }
        public GameSearcher Searcher { get; set; }

        public ConfirmGame_Button AuthorizeButton { get; protected set; }
        public HoverDesignButton CancelButton { get; protected set; }

        public StackPanel UpperPanel { get; protected set; }
        public StackPanel ButtonsPanel { get; protected set; }

        public GameDetector_Control(IGameDefault game) : base()
        {
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            Orientation = Orientation.Horizontal;
            GameDefault = game;

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
                Text = GameDefault.Name,
                FontSize = 20,
                Margin = new Thickness(0,0,0,10),
                Foreground = Brushes.LightGray
            });

            UpperPanel.Children.Add(new TextBox()
            {
                IsReadOnly = true,
                IsEnabled = false,
                Background = Brushes.Gray,
                Height = 25,
                Margin = new Thickness(0, 0, 0, 10)
            });

            ButtonsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            AuthorizeButton = new ConfirmGame_Button(GameDefault.Name, Defined.MODPICKINGBUTTON_SIZE, Defined.MODPICKINGBUTTON_SIZE);
            AuthorizeButton.Click += new RoutedEventHandler(ConfirmGameButton_Click);

            CancelButton = new HoverDesignButton()
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0,0,0,10),
                Width = Defined.MODPICKINGBUTTON_SIZE,
                Height = Defined.MODPICKINGBUTTON_SIZE,
                MouseNotOverDesign = Assistant.LoadImageFromResources("Button_RedX"),
                MouseOverDesign = Assistant.LoadImageFromResources("Button_RedX_Hover")
            };
            CancelButton.Click += new RoutedEventHandler(IgnoreModButton_Click);


            ButtonsPanel.Children.Add(CancelButton);
            ButtonsPanel.Children.Add(AuthorizeButton);

            ResetGUI();

            UpperPanel.Children.Add(ButtonsPanel);

            Children.Add(new Image()
            {
                Width = 140,
                Source = Assistant.LoadImageFromResources("Icon - " + GameDefault.Id.ToString() + ".png"),
                Height = 140,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Top
            });
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

                CancelButton.ChangeState(false);
                AuthorizeButton.ChangeState(false);
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

            AuthorizeButton.ChangeState(false);
            CancelButton.ChangeState(true);
            AuthorizeButton.Visibility = Visibility.Hidden;
        }

        public void FoundGame()
        {
            AuthorizeButton.ChangeState(true);
        }

        public void ConfirmGame()
        {
            CancelButton.ChangeState(false);
            AuthorizeButton.ChangeState(false);
            if (ButtonsPanel.Children[ButtonsPanel.Children.Count - 1].GetType() == typeof(TextBlock))
            {
                ButtonsPanel.Children.RemoveAt(ButtonsPanel.Children.Count - 1);
            }

            ButtonsPanel.Children.Add(new TextBlock()
            {
                Text = "Game Confirmed!",
                Margin = new Thickness(35, 0, 0, 0),
                Foreground = Brushes.Goldenrod,
                FontSize = GameSearcher.GameStateTextSize,
                VerticalAlignment = VerticalAlignment.Center,
            });

            Searcher.ConfirmGame();

            if (Searcher.ConfirmedGames.Count == 1)
            {
                Searcher.AssociatedGame.ToggleIsCurrentGame();
            }
        }

        private void ConfirmGameButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmGame();
        }

    }
}
