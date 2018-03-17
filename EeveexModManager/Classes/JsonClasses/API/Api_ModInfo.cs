using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace EeveexModManager.Classes.JsonClasses.API
{
    public class Api_ModInfo
    {
        public string name { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public int category_id { get; set; }
        public string version { get; set; }
        public string author { get; set; }

        [JsonProperty("contains_adult_content?")]
        public bool contains_adult_content { get; set; }
        public string uploaded_by { get; set; }
        public string picture_url { get; set; }
    }
}
