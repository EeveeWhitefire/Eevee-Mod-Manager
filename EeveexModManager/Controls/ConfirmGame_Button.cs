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
    public class ConfirmGame_Button : SquareButton
    {
        public string AssociatedGame { get; protected set; }

        public ConfirmGame_Button(string game, double radX, double radY) : base(radX, radY, new Image() {
            Width = Defined.MODPICKINGBUTTON_SIZE,
            Height = Defined.MODPICKINGBUTTON_SIZE,
            Source = new BitmapImage(new Uri("pack://application:,,,/EeveexModManager;component/Resources/Button_GreenCheckMark_Hover.png", UriKind.Absolute)),
            VerticalAlignment = VerticalAlignment.Center})

        {
            AssociatedGame = game;
            VerticalAlignment = VerticalAlignment.Bottom;
            Margin = new Thickness(10, 0, 0, 10);
            //Visibility = Visibility.Hidden;
            Content = new Image
            {
                Width = Defined.MODPICKINGBUTTON_SIZE,
                Height = Defined.MODPICKINGBUTTON_SIZE,
                Source = new BitmapImage(new Uri("pack://application:,,,/EeveexModManager;component/Resources/Button_GreenCheckMark.png", UriKind.Absolute)),
                VerticalAlignment = VerticalAlignment.Center
            };
        }

    }
}
