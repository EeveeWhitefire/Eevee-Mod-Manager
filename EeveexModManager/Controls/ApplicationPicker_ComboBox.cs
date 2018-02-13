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
        Action Utility;
        List<Db_GameApplication> Apps;

        public ApplicationPicker_ComboBox(List<Db_GameApplication> apps, Action OnUtility) : base()
        {
            HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Bottom;
            Height = 80;
            Width = 400;
            Init(apps);
            Utility = OnUtility;

            SelectionChanged += new SelectionChangedEventHandler(SelectionChanged_Event);
        }

        public void Init(List<Db_GameApplication> apps)
        {
            Items.Add(new TextBlock()
            {
                Text = "Add another application",
                FontSize = 20,
                Height = 50,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            Apps = apps;
            foreach (var a in Apps.Select(x => x.EncapsulateToSource()))
            {
                Items.Add(new ApplicationPicker_Control(a));
            }

            SelectedIndex = 1;
        }

        void SelectionChanged_Event(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            if (index == 0)
            {
                SelectedIndex = 1;
                Utility();
            }
            else
            {
            }
        }
    }
}
