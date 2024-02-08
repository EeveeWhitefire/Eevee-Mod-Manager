using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Linq;

namespace EeveexModManager.Services
{
    public class Service_JsonParser
    {
        private string configPath;
        public Service_JsonParser()
        {
            configPath = Directory.GetCurrentDirectory() + @"\config.json";
        }

        public string LoadFile(string path)
        {
            string result;
            using (StreamReader reader = File.OpenText(path))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        public void UpdateJson<T>(T data, string Path = null)
        {
            Path = Path ?? configPath;
            string output = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(Path, output);
        }

        public async Task<T> GetJsonFields<T>(Uri url, string ContentType = null, string authToken = null)
        {
            var extractedData = JsonConvert.DeserializeObject<T>(await GetJSONstringFromURL(url, ContentType, authToken));
            return extractedData;
        }


        public T GetJsonFields<T>(string path = "", string name = null)
        {
            name = name ?? configPath;
            if (File.Exists(path + name))
            {
                var extractedData = JsonConvert.DeserializeObject<T>(LoadFile(path + name));
                return extractedData;
            }
            else
            {
                return default(T);
            }
        }

        private async Task<string> GetJSONstringFromURL(Uri url, string ContentType = null, string authToken = null)
        {
            var httpClient = new HttpClient();
            if(ContentType != null)
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", ContentType);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", ContentType);
                if(authToken != null)
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            }
            var response = await httpClient.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}
