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
            /*
                    <StackPanel HorizontalAlignment="Left" VerticalAlignment="Top" Orientation="Horizontal">
                        <Image Source="{StaticResource icon_skyrimSe}" Width="80" Height="80" Margin="0,0,10,0" VerticalAlignment="Top"/>
                        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Top" Orientation="Vertical" Margin="0,0,20,0" Width="160">
                            <TextBlock Text="TESV : Skyrim Special Edition" Margin="0,0,0,10"/>
                            <TextBox IsReadOnly="True" Margin="0,0,0,10"/>
                            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                                <Button VerticalAlignment="Bottom" Margin="0,0,0,10" Visibility="Hidden">
                                    <Image Source="{StaticResource button_greenCheck}" Width="20" Uid="ConfirmGame"/>
                                </Button>
                                <Button VerticalAlignment="Bottom" Margin="10,0,0,10" Click="IgnoreModButton_Click">
                                    <Image Source="{StaticResource button_redX}" Width="20" Uid="Cancel"/>
                                </Button>
                            </StackPanel>
                        </StackPanel>
                    </StackPanel>*/
            StackPanel firstPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 10, 0),
                Width = 200
            };
            
            firstPanel.Children.Add(new TextBlock()
            {
                Text = GameName,
                Margin = new Thickness(0,0,0,10)
            });

            ProgressBar = new TextBox()
            {
                IsReadOnly = true,
                IsEnabled = false,
                Margin = new Thickness(0, 0, 0, 10)
            };


            firstPanel.Children.Add(ProgressBar);

            StackPanel secondPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            CancelButton = new Button()
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(10,0,0,10),
                Content = new Image
                {
                    Width = 50,
                    Height = 50,
                    Source = new BitmapImage(new Uri("pack://application:,,,/EeveexModManager;component/Resources/Button_RedX.png", UriKind.Absolute)),
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            secondPanel.Children.Add(CancelButton);

            AuthorizeButton = new ConfirmGame_Button(GameName);

            AuthorizeButton.Click += new RoutedEventHandler(ConfirmGame_Click);

            secondPanel.Children.Add(AuthorizeButton);

            firstPanel.Children.Add(secondPanel);
            
            Image icon = new Image()
            {
                Width = 130,
                Source = new BitmapImage(new Uri($"pack://application:,,,/EeveexModManager;component/Resources/Icon - {GameName.Replace(" : ", " ")}.png", UriKind.Absolute)),
                Height = 130,
                Margin = new Thickness(0,0,10,0),
                VerticalAlignment = VerticalAlignment.Top
            };
            Children.Add(icon);
            Children.Add(firstPanel);

        }

        public void FoundGame()
        {
            AuthorizeButton.IsEnabled = true;
        }

        private void ConfirmGame_Click(object sender, RoutedEventArgs e)
        {
            CancelButton.IsEnabled = false;
            Searcher.Confirmed = true;
        }

    }
}
