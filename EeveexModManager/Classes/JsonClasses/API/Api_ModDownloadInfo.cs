using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Classes.JsonClasses.API
{
    public class Api_ModDownloadInfo
    {
        public string URI { get; set; }
        public string Country { get; set; }
        public bool IsPremium { get; set; }
        public string Name { get; set; }
        public int ConnectedUsers { get; set; }
    }
}
