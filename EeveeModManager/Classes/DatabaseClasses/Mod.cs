using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Interfaces;
using EeveexModManager.Classes;
using LiteDB;
using System.IO;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class Mod : IMod
    {
        public string Name { get; set; }
        public string ModFileName { get; set; }
        public bool Active { get; set; }
        public bool Installed { get; set; }
        public string SourceArchive { get; set; }
        public string ModDirectory { get; set; }
        public string DownloadDirectory { get; }
        public string Version { get; set; }
        public string Id { get; set; }
        public bool IsOnline { get; set; }
        public string Author { get; set; }
        public string FullSourceUri { get; set; }
        public int Priority { get; set; }

        public GameListEnum GameId { get; set; }
        public ModCategories ModCategory { get; set; }

        [BsonId]
        public string FileId { get; set; }

        public BinaryNode<string> FileTree;
        public IEnumerable<string> Plugins
        {
            get { return FileTree.Children.Where(x => x.IsStandalone && x.Value.ToLower().EndsWith(".esp")).Select(x => x.Value); }
        }
        #region Constructors
        public Mod() { }

        public Mod(string n, string fileN, bool active, bool installed, string source, string modDir, string dlDir, GameListEnum gameId, ModCategories category, string fileId, int priority,
            string version = Defined.DEFAULTMODVERSION, string id = Defined.DEFAULTMODID, string author = Defined.DEFAULTMODAUTHOR, string srcUri = Defined.DEFAULTSOURCEURI,
            bool isOn = false)
        {
            Name = n;
            IsOnline = isOn;
            ModFileName = fileN;
            Active = active;
            Installed = installed;
            SourceArchive = source;
            ModDirectory = modDir;
            DownloadDirectory = dlDir;
            GameId = gameId;
            ModCategory = category;
            FileId = fileId;
            Version = version;
            Id = id;
            Author = author;
            FullSourceUri = srcUri;
            Priority = priority;

            FileTree = BinaryNode.GetDirectoryTree(ModDirectory);
        }
        #endregion

        public void ToggleIsActive()
        {
            Active = !Active;
        }

        public string GetUrl(string gameName)
            => $@"https://www.nexusmods.com/{gameName}/mods/{Id}";
    }
}
