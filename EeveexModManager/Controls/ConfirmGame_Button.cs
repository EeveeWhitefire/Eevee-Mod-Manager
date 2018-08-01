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
    public class ConfirmGame_Button : HoverDesignButton
    {
        public string AssociatedGame { get; protected set; }

        public ConfirmGame_Button(string game, double width, double height) : base()
        {
            Width = width;
            Height = height;
            AssociatedGame = game;
            VerticalAlignment = VerticalAlignment.Bottom;
            Margin = new Thickness(10, 0, 20, 10);
            MouseOverDesign = Assistant.LoadImageFromResources("Button_GreenCheckMark_Hover");
            MouseNotOverDesign = Assistant.LoadImageFromResources("Button_GreenCheckMark");
        }
    }
}
