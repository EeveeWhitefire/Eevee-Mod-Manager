using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EeveexModManager.Classes
{
    public class ModArchiveFile
    {
        public string Extension { get; set; }

        ArchiveType FileType;

        public string FileName { get; set; }
        public string Path { get; set; }
        public string FullSourcePath { get; set; }
        public string FullNewPath { get; set; }
        public string ModDirectory { get; set; }

        public ModArchiveFile(Game game, string fullPath)
        {
            FileInfo x = new FileInfo(fullPath);

            Extension = x.Extension;
            FileName = x.Name;
            ModDirectory = $@"Mods\{game.Name}\off-{FileName}";
            FullSourcePath = fullPath;

            FullNewPath = $@"Downloads\{game.Name}\off-{FileName}{Extension}";
            File.Copy(FullSourcePath, FullNewPath);



            switch (Extension)
            {
                case ".rar":
                    FileType = ArchiveType.Rar;
                    break;
                case ".zip":
                    FileType = ArchiveType.Zip;
                    break;
                case ".7z":
                    FileType = ArchiveType.SevenZip;
                    break;
                default:
                    break;
            }
        }
        public void Extract()
        {


            switch (FileType)
            {
                case ArchiveType.Rar:
                    break;
                case ArchiveType.SevenZip:
                    SevenZipBase.SetLibraryPath(@"DLLs\7z.dll");
                    SevenZipExtractor extractor = new SevenZipExtractor(FullNewPath);
                    extractor.ExtractArchive(ModDirectory);
                    break;
                case ArchiveType.Zip:
                    ZipFile.ExtractToDirectory(FullNewPath, ModDirectory);
                    break;
                default:
                    break;
            }
        }

        enum ArchiveType
        {
            Rar,
            SevenZip,
            Zip
        }
    }
}
