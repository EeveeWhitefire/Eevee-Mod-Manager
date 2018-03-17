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
    public class ConfirmGame_Button : SquareButton
    {
        public string AssociatedGame { get; protected set; }

        public ConfirmGame_Button(string game, double radX, double radY) : base(radX, radY, "Button_GreenCheckMark" )
        {
            AssociatedGame = game;
            VerticalAlignment = VerticalAlignment.Bottom;
            Margin = new Thickness(10, 0, 20, 10);
        }
    }
}
