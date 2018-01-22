using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager
{
    public interface IModInfo
    {
        #region Properties
        
        string Id { get; set; }
        
        string ModName { get; }
        
        string HumanReadableVersion { get; }
        
        string LastKnownVersion { get; }
        
        bool? IsEndorsed { get; }
        
        Version MachineVersion { get; }
        
        string Author { get; }
        
        Int32 CategoryId { get; }
        
        Int32 CustomCategoryId { get; }
        
        string Description { get; }
        
        string InstallDate { get; set; }
        
        Uri Website { get; }
        
        //ExtendedImage Screenshot { get; }
        
        bool UpdateWarningEnabled { get; }

        #endregion
        
        void UpdateInfo(IModInfo modInfo, bool overwriteAllValues);
    }
}
