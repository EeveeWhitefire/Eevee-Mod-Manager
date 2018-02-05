using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Interfaces
{
    public interface IDownload
    {
        string ModName { get; }
        string ModFileName { get; }
        Uri DownloadFrom { get; }
        string DownloadTo { get; }
        string DownloadAt { get; }
        string InstallAt { get; }
    }
}
