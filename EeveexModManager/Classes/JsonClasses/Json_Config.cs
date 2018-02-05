using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Classes;

namespace EeveexModManager.Classes.JsonClasses
{
    public class Json_Config
    {
        public StatesOfConfiguartion State { get; set; }  = StatesOfConfiguartion.FirstTime;
        public bool Nxm_Handled { get; set; } = false;
        public string Installation_Path { get; set; } = Directory.GetCurrentDirectory();
    }
}
