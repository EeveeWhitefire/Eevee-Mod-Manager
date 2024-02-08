using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

using EeveexModManager.Classes.NexusClasses.API;

namespace EeveexModManager.Interfaces
{
    public interface INexusCommunicationManager
    {
        HttpClient HttpCommunicator { get; }
        string Token { get; }

        Task<T> GET<T>(Uri url);
        Task<Api_ModDownloadInfo> GetDownloadLinks(string gameName, string modId, string fileId);
        Task<Api_ModInfo> GetModInfo(string gameName, string modId);
        Task<Api_ModFileInfo> GetModFileInfo(string gameName, string modId, string fileId);
    }
}
