using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace EeveexModManager.Controls
{
    public class SquareButton : ShapedButton<Rectangle>
    {
        public SquareButton(double radX, double radY, string baseTextureName) : base(radX, radY, baseTextureName)
        {
        }
    }
}
