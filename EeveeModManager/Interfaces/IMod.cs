using EeveexModManager.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Interfaces
{
    public enum ModCategories
    {
        Unknown
    }

    interface IMod
    {
        string Name { get; }
        string ModFileName { get; }
        bool IsOnline { get; }
        bool Active { get; }
        bool Installed { get; }
        string SourceArchive { get; }
        string ModDirectory { get; }
        string DownloadDirectory { get; }
        string Version { get; }
        string Id { get; }
        string Author { get; }
        string FullSourceUri { get; }

        GameListEnum GameId { get; }
        ModCategories ModCategory { get; }
        string FileId { get; set; }
       
    }
}
