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
    public class ConfirmGame_Button : Button
    {
        public string AssociatedGame { get; protected set; }

        public ConfirmGame_Button(string game) : base()
        {
            AssociatedGame = game;
            VerticalAlignment = VerticalAlignment.Bottom;
            Margin = new Thickness(0, 0, 0, 10);
            Visibility = Visibility.Hidden;
            Content = new Image
            {
                Width = 50,
                Height = 50,
                Source = new BitmapImage(new Uri("pack://application:,,,/EeveexModManager;component/Resources/Button_GreenCheckMark.png", UriKind.Absolute)),
                VerticalAlignment = VerticalAlignment.Center
            };
        }

    }
}
