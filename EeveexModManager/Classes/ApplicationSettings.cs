using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Classes
{
    public enum StatesOfConfiguration
    {
        FirstTime,
        OnPickingCurrentGame,
        Ready
    }

    //Application settings wrapper class
    public sealed class ApplicationSettings : ApplicationSettingsBase
    {
        public ApplicationSettings() : base()
        {
            InstallationPath = InstallationPath ?? Environment.CurrentDirectory;
            Environment.CurrentDirectory = InstallationPath;
            ApplicationDataPath = ApplicationDataPath ?? $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\EMM";
        }

        [UserScopedSetting()]
        public String InstallationPath
        {
            get { return (String)this["InstallationPath"]; }
            set { this["InstallationPath"] = value; }
        }

        [UserScopedSetting()]
        public String ApplicationDataPath
        {
            get { return (String)this["ApplicationDataPath"]; }
            set { this["ApplicationDataPath"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("false")]
        public Boolean IsNxmHandled
        {
            get { return (Boolean)this["IsNxmHandled"]; }
            set { this["IsNxmHandled"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("0")]
        public StatesOfConfiguration State
        {
            get { return (StatesOfConfiguration)this["State"]; }
            set { this["State"] = value; }
        }
    }
}
