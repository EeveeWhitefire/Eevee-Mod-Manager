using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EeveexModManager.Controls
{
    public class CircularButton : ShapedButton<Ellipse>
    {
        public CircularButton(double radX, double radY, string baseTextureName) : base(radX, radY, baseTextureName)
        {
        }
    }
}
