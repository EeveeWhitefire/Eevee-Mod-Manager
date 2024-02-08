using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Controls
{
    public class GamePicker_Control : TextBlock
    {
        public Game AssociatedGame { get; protected set; }
        
        public GamePicker_Control(Game game) : base()
        {
            AssociatedGame = game;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Center;
            Text = AssociatedGame.Name;
        }
    }
}