using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EeveexModManager.Interfaces;

namespace EeveexModManager.Classes.JsonClasses.API
{
    public class Api_ModInfo
    {
        public string version { get; set; }
        public string author { get; set; }
        public ModCategories category { get; set; }
    }
}
