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
        public string Extension { get; }

        ArchiveType FileType;

        public string FileName { get; }
        public string Path { get; }
        public string FullSourcePath { get; }
        public string FullNewPath { get; }
        public string ModDirectory { get; }
        public string DownloadDirectory { get; }

        public ModArchiveFile(Game game, string fullPath, string modName)
        {
            FileInfo x = new FileInfo(fullPath);

            Extension = x.Extension;
            FileName = x.Name;
            ModDirectory = $@"{game.ModsDirectory}\{modName}";
            DownloadDirectory = $@"{game.DownloadsDirectory}\{modName}";
            FullSourcePath = fullPath;

            FullNewPath = fullPath;

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

        public ModArchiveFile(string fullPath, string installTo)
        {
            FullNewPath = fullPath;
            FileInfo x = new FileInfo(fullPath);

            Extension = x.Extension;
            FileName = x.Name;
            ModDirectory = installTo;

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
