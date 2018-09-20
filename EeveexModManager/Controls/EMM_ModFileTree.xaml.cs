using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Controls
{
    /// <summary>
    /// Interaction logic for EMM_ModFileTree.xaml
    /// </summary>
    public partial class EMM_ModFileTree : UserControl
    {
        public Mod AssociatedMod { get; protected set; }
        public EMM_ModFileTree(Mod m)
        {
            InitializeComponent();
            AssociatedMod = m;
        }
    }
}
