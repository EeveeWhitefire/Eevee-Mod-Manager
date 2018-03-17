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
using EeveexModManager.Classes.JsonClasses;
using EeveexModManager.Services;

namespace EeveexModManager.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private DatabaseContext_Main _db;
        private Json_Config _config;
        private NxmHandler _nxmHandler;
        private Service_JsonParser _jsonParser;

        public SettingsWindow(Json_Config cnfg, DatabaseContext_Main db, Service_JsonParser parser)
        {
            _db = db;
            _config = cnfg;
            InitializeComponent();
            AssociationWithNXM_CheckBox.IsChecked = _config.Nxm_Handled;
            _jsonParser = parser;

            _nxmHandler = new NxmHandler(_config, _jsonParser, AssociationWithNXM_CheckBox, _db);
        }

        ~SettingsWindow()
        {
            try
            {
                if (_config.Nxm_Handled != AssociationWithNXM_CheckBox.IsChecked)
                {
                    _nxmHandler.AssociationManagement(_config.Nxm_Handled, AssociationWithNXM_CheckBox, _db.GetCollection<Db_Game>("games").FindAll());
                }
            }
            catch (Exception)
            {
                //Close();
            }
        }

        private void Association_Button_Click(object sender, RoutedEventArgs e)
        {
            _nxmHandler.AssociationManagement(_config.Nxm_Handled, AssociationWithNXM_CheckBox, _db.GetCollection<Db_Game>("games").FindAll());
        }
    }
}
