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
using System.Windows.Shapes;

using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Services;

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private DatabaseContext_Main _db;
        private NxmHandler _nxmHandler;
        private Service_JsonParser _jsonParser;

        public SettingsWindow(DatabaseContext_Main db, Service_JsonParser parser)
        {
            _db = db;
            InitializeComponent();
            AssociationWithNXM_CheckBox.IsChecked = Defined.Settings.IsNxmHandled;
            _jsonParser = parser;

            _nxmHandler = new NxmHandler(_jsonParser, AssociationWithNXM_CheckBox, _db);
        }

        ~SettingsWindow()
        {
            try
            {
                if (Defined.Settings.IsNxmHandled != AssociationWithNXM_CheckBox.IsChecked)
                {
                    _nxmHandler.AssociationManagement(Defined.Settings.IsNxmHandled, AssociationWithNXM_CheckBox, _db.GetCollection<Db_Game>("games").FindAll());
                }
            }
            catch (Exception)
            {
                //Close();
            }
        }

        private void Association_Button_Click(object sender, RoutedEventArgs e)
        {
            _nxmHandler.AssociationManagement(Defined.Settings.IsNxmHandled, AssociationWithNXM_CheckBox, _db.GetCollection<Db_Game>("games").FindAll());
        }
    }
}
