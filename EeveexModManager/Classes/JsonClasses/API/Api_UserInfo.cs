using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace EeveexModManager.Classes.JsonClasses.API
{
    public class Api_UserInfo
    {
        public string name { get; set; }
        public string key { get; set; }
        public ulong user_id { get; set; }
        [JsonProperty("is_premium?")]
        public bool is_premium { get; set; }
        [JsonProperty("is_supporter?")]
        public bool is_supporter { get; set; }
    }
}
