using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Classes.NexusClasses.API;
using EeveexModManager.Interfaces;

using Newtonsoft.Json;

namespace EeveexModManager.Classes
{
    public class NexusCommunicationManager : INexusCommunicationManager
    {
        public HttpClient HttpCommunicator { get; protected set; }

        public string Token { get; protected set; }

        public NexusCommunicationManager(string token)
        {
            Token = token;
            HttpCommunicator = new HttpClient();
            HttpCommunicator.DefaultRequestHeaders.Add("APIKEY", Token);
            HttpCommunicator.DefaultRequestHeaders.Add("User-Agent", "Nexus Client v0.63.14");
        }

        public async Task<T> GET<T>(Uri url)
        {
            string res = await(await HttpCommunicator.GetAsync(url)).Content.ReadAsStringAsync();
            res = res.Replace("[", string.Empty).Replace("]", string.Empty); //removing first and last '[' ']' cus i don't see any reason for it to be a list
            T result = JsonConvert.DeserializeObject<T>(res);

            return result;
        }

        public async Task<Api_ModDownloadInfo> GetDownloadLinks(string gameName, string modId, string fileId)
        {
            Uri uri = new Uri($"{Defined.NEXUSAPI_BASE}/games/{gameName}/mods/{modId}/files/{fileId}/download_link");
            return await GET<Api_ModDownloadInfo>(uri);
        }

        public async Task<Api_ModInfo> GetModInfo(string gameName, string modId)
        {
            Uri uri = new Uri($"{Defined.NEXUSAPI_BASE}/games/{gameName}/mods/{modId}");
            return await GET<Api_ModInfo>(uri);
        }

        public async Task<Api_ModFileInfo> GetModFileInfo(string gameName, string modId, string fileId)
        {
            Uri uri = new Uri($"{Defined.NEXUSAPI_BASE}/games/{gameName}/mods/{modId}/files/{fileId}");
            return await GET<Api_ModFileInfo>(uri);
        }
    }
}
