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
using EeveexModManager.Classes;

namespace EeveexModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<Mod> mods = new List<Mod>
            {
                new Mod {Active=false, Id=9999, Installed=false, Name="test", Version="6.9.4"},
                new Mod {Active=false, Id=9999, Installed=false, Name="test", Version="6.9.4"},
                new Mod {Active=false, Id=9999, Installed=false, Name="test", Version="6.9.4"},
                new Mod {Active=false, Id=9999, Installed=false, Name="test", Version="6.9.4"},
            };
            mods.ForEach(mod =>
            {
                var x = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                var z = new BitmapImage(new Uri("pack://application:,,,/Images/test.png"));
                
                x.Children.Add(new Image() { Source = z, Width=20});
                x.Children.Add(new TextBlock() { Text = $"{mod.Name}        Active:{mod.Active}     Id:{mod.Id}     Version:{mod.Version}" });

                ModelsAndTexturesItem.Items.Add(x);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked on launch");
        }
    }
}
