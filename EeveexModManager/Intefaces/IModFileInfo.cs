using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager
{
    public interface IModFileInfo
    {
        #region Properties
        
        string Id { get; }
        
        string Filename { get; }
        
        string Name { get; }
        
        string HumanReadableVersion { get; }
        #endregion
    }
}
