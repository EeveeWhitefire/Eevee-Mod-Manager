using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EeveexModManager.Classes;

namespace EeveexModManager.Controls
{
    public class GamePicker_Control : StackPanel
    {
        public Game AssociatedGame { get; protected set; }

        StackPanel stkPanel;
        double TextSize;

        public GamePicker_Control(Game game, double imageSize, double fontSize) : base()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Horizontal;

            AssociatedGame = game;
            TextSize = fontSize;

            Image icon = new Image()
            {
                Width = imageSize,
                Source = new BitmapImage(new Uri($"pack://application:,,,/EeveexModManager;component/Resources/Icon - {AssociatedGame.Name.Replace(" : ", " ")}.png", UriKind.Absolute)),
                Height = imageSize,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            stkPanel = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Vertical
            };


            Children.Add(icon);

            stkPanel.Children.Add(new TextBlock()
            {
                Text = AssociatedGame.Name,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = TextSize
            });

            if ( game.IsCurrent)
            {
                GameToCurrent();
            }

            Children.Add(stkPanel);
        }

        public void GameToCurrent(bool toCurr = true)
        {
            if (AssociatedGame.IsCurrent != toCurr)
            {
                AssociatedGame.ToggleIsCurrentGame();
            }

            if (toCurr)
            {
                stkPanel.Children.Add(new TextBlock()
                {
                    Text = "Current",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Foreground = Brushes.Green,
                    FontSize = TextSize - 5
                });
            }
            else
            {
                stkPanel.Children.RemoveAt(stkPanel.Children.Count - 1);
            }

        }
    }
}