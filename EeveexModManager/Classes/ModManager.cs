using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

using Newtonsoft.Json;

using SevenZip;

using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Classes.NexusClasses.API;
using EeveexModManager.Interfaces;
using EeveexModManager.Nexus;
using EeveexModManager.Controls;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Threading;

namespace EeveexModManager.Classes
{
    public class ModManager
    {
        private DatabaseContext_Profile _db;
        private HttpClient httpClient;

        private ListView Mods_View { get; }
        private ListView Downloads_View { get; }
        public List<Mod_Control> ModControls { get; protected set; }
        private AccountHandler _accountHandler;

        public DownloadManager DownloadsManager { get; protected set; }

        public ModManager(DatabaseContext_Profile db)
        {
            httpClient = new HttpClient();
            _db = db;
        }


        public ModManager(DatabaseContext_Profile db, ListView modsTreeView, ListView downloadsTreeView) : 
            this(db)
        {
            httpClient = new HttpClient();
            DownloadsManager = new DownloadManager(downloadsTreeView, AddModToGUI);


            Downloads_View = downloadsTreeView;
            Mods_View = modsTreeView;
            ModControls = new List<Mod_Control>();
            Mods_View.ItemsSource = ModControls;
        }

        public void UpdateMod(Mod m)
        {
            _db.GetCollection<Db_Mod>("mods").Update(m.EncapsulateToDb());
        }

        public void RemoveMod(string fileId, int index)
        {
            _db.GetCollection<Db_Mod>("mods").Delete(x => x.FileId == fileId);
            ModControls.RemoveAt(index);
            Mods_View.Items.Refresh();
        }

        public void ChangeProfile(DatabaseContext_Profile db)
        {
            _db = db;
            ModControls.Clear();
            _db.GetCollection<Db_Mod>("mods").FindAll().ToList().ForEach(x =>
            {
                ModControls.Add(new Mod_Control(x.EncapsulateToSource(), _db));
            });
        }
        public void SetAccountHandler(AccountHandler ah)
        {
            _accountHandler = ah;
        }

        public void AddModToGUI(Mod m)
        {
            _db.GetCollection<Db_Mod>("mods").Insert(m.EncapsulateToDb());
            ModControls.Add(new Mod_Control(m, _db));
            Mods_View.Items.Refresh();
        }
        
        public void CreateModFromArchive(Game CurrentGame, ModArchiveFile archive) //offline
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Archive Files|*.rar;*.zip;*.7z;"
            };
            int modIndex = _db.GetCollection<Db_Mod>("mods").Count();
            
            Mod newMod = new Mod($"Offline Mod #{modIndex}", archive.FileName, false, true, archive.FullNewPath, archive.ModDirectory, archive.DownloadDirectory,
                CurrentGame.Id, ModCategories.Unknown, $"off-{modIndex}", ModControls.Count);

            archive.Extract();
            AddModToGUI(newMod);
        }
        public async Task CreateMod(Game CurrentGame, NexusUrl nexusUrl) //online
        {
            if (!_db.GetCollection<Db_Mod>("mods").FindAll().Select(x => x.FileId).Contains(nexusUrl.FileId) && _accountHandler.isLoggedIn)
            {
                string API_DownloadLinkSource = $"{Defined.NEXUSAPI_BASE}/games/{CurrentGame.Name_API}/mods/{nexusUrl.ModId}/files/{nexusUrl.FileId}/download_link";
                string API_ModInfoSource = $"{Defined.NEXUSAPI_BASE}/games/{CurrentGame.Name_API}/mods/{nexusUrl.ModId}";
                string API_ModFileInfoSource = $"{Defined.NEXUSAPI_BASE}/games/{CurrentGame.Name_API}/mods/{nexusUrl.ModId}/files/{nexusUrl.FileId}";

                httpClient.DefaultRequestHeaders.Add("APIKEY", _accountHandler.token);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Nexus Client v0.63.14");


                string res1 = await (await httpClient.GetAsync(API_DownloadLinkSource)).Content.ReadAsStringAsync();
                res1 = res1.Replace("[", string.Empty).Replace("]", string.Empty); //removing first and last '[' ']' cus i don't see any reason for it to be a list

                string res2 = await (await httpClient.GetAsync(API_ModInfoSource)).Content.ReadAsStringAsync();
                res2 = res2.Replace("[", string.Empty).Replace("]", string.Empty);

                string res3 = await (await httpClient.GetAsync(API_ModFileInfoSource)).Content.ReadAsStringAsync();
                res3 = res3.Replace("[", string.Empty).Replace("]", string.Empty);

                var result_downloadInfo = JsonConvert.DeserializeObject<Api_ModDownloadInfo>(res1);
                var result_modInfo = JsonConvert.DeserializeObject<Api_ModInfo>(res2);
                var result_fileInfo = JsonConvert.DeserializeObject<Api_ModFileInfo>(res3);

                string ModDownloadTo = $@"{CurrentGame.DownloadsDirectory }\{result_modInfo.name}";
                if (!Directory.Exists(ModDownloadTo))
                {
                    Directory.CreateDirectory(ModDownloadTo);
                }

                string DownloadTo = $@"{ModDownloadTo}\{ result_fileInfo.name.Split('-').LastOrDefault().Trim(' ')}";
                string DownloadAs = DownloadTo + $@"\{result_fileInfo.file_name}";
                string InstallTo = $@"{CurrentGame.ModsDirectory}\{result_modInfo.name} [{result_fileInfo.name}]";

                if (!Directory.Exists(DownloadTo))
                    Directory.CreateDirectory(DownloadTo);
                if (!Directory.Exists(InstallTo))
                    Directory.CreateDirectory(InstallTo);

                if (File.Exists(DownloadAs))
                    File.Delete(DownloadAs);

                Mod newMod = new Mod(result_modInfo.name, result_fileInfo.name, false, true, DownloadAs, InstallTo, DownloadTo,
                    CurrentGame.Id, (ModCategories)result_modInfo.category_id, nexusUrl.FileId, ModControls.Count, result_modInfo.version, nexusUrl.ModId,
                    result_modInfo.author, nexusUrl.SourceUrl.ToString(), true);

                await Task.Run(() =>
                Downloads_View.Dispatcher.Invoke(() =>
                {
                    DownloadsManager.AddDownload(new Uri(result_downloadInfo.URI), DownloadAs, DownloadTo, InstallTo, result_modInfo.name, result_fileInfo.name, newMod);
                }, DispatcherPriority.ApplicationIdle));
            }
        }

        //for testing purpose only, accept any dodgy certificate... 
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
