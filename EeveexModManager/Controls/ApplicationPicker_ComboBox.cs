using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace EeveexModManager.Controls
{
    public class ApplicationPicker_ComboBox : ComboBox
    {
        IEnumerable<GameApplication> Apps;
        private List<ApplicationPicker_Control> _controls;

        public ApplicationPicker_ComboBox(ComboBox info) : base()
        {
            HorizontalAlignment = info.HorizontalAlignment;
            VerticalAlignment = info.VerticalAlignment;
            Height = info.Height;
            Width = info.Width;
        }

        public void Init(IEnumerable<GameApplication> apps)
        {
            Apps = apps;
            _controls = Apps.Select(x => new ApplicationPicker_Control(x)).ToList();
            ItemsSource = _controls;
            Items.Refresh();
            SelectedIndex = 0;
        }
    }
}
