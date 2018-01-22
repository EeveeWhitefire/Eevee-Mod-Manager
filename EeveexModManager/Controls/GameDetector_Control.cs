using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using EeveexModManager.Classes;
using EeveexModManager.Interfaces;
using EeveexModManager.Define;

namespace EeveexModManager.Controls
{
    public class GameDetector_Control : StackPanel
    {

        public string GameName { get; protected set; }
        public GameSearcher Searcher { get; set; }

        public ConfirmGame_Button AuthorizeButton { get; protected set; }
        public Button CancelButton { get; protected set; }
        public TextBox ProgressBar { get; protected set; }

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
            StackPanel firstPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 10, 0),
                Width = 280
            };
            
            firstPanel.Children.Add(new TextBlock()
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


            firstPanel.Children.Add(ProgressBar);

            StackPanel secondPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            CancelButton = new SquareButton(Defined.MODPICKINGBUTTON_SIZE, Defined.MODPICKINGBUTTON_SIZE, new Image
            {
                Width = Defined.MODPICKINGBUTTON_SIZE,
                Height = Defined.MODPICKINGBUTTON_SIZE,
                Source = new BitmapImage(new Uri("pack://application:,,,/EeveexModManager;component/Resources/Button_RedX_Hover.png", UriKind.Absolute)),
                VerticalAlignment = VerticalAlignment.Center
            })
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(10,0,0,10),
                Content = new Image
                {
                    Width = Defined.MODPICKINGBUTTON_SIZE,
                    Height = Defined.MODPICKINGBUTTON_SIZE,
                    Source = new BitmapImage(new Uri("pack://application:,,,/EeveexModManager;component/Resources/Button_RedX.png", UriKind.Absolute)),
                    VerticalAlignment = VerticalAlignment.Center
                },
            };
            CancelButton.Click += new RoutedEventHandler(IgnoreModButton_Click);

            secondPanel.Children.Add(CancelButton);

            AuthorizeButton = new ConfirmGame_Button(GameName, Defined.MODPICKINGBUTTON_SIZE, Defined.MODPICKINGBUTTON_SIZE);

            AuthorizeButton.Click += new RoutedEventHandler(ConfirmGameButton_Click);

            secondPanel.Children.Add(AuthorizeButton);

            firstPanel.Children.Add(secondPanel);
            
            Image icon = new Image()
            {
                Width = 140,
                Source = new BitmapImage(new Uri($"pack://application:,,,/EeveexModManager;component/Resources/Icon - {GameName.Replace(" : ", " ")}.png", UriKind.Absolute)),
                Height = 140,
                Margin = new Thickness(0,0,10,0),
                VerticalAlignment = VerticalAlignment.Top
            };
            Children.Add(icon);
            Children.Add(firstPanel);

        }


        private void IgnoreModButton_Click(object sender, RoutedEventArgs e)
        {
            Searcher.Confirmed = false;
        }

        public void FoundGame()
        {
            AuthorizeButton.IsEnabled = true;
        }

        private void ConfirmGameButton_Click(object sender, RoutedEventArgs e)
        {
            CancelButton.IsEnabled = false;
            Searcher.Confirmed = true;
        }

    }
}
