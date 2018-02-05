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

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

using Newtonsoft.Json;

using SevenZip;

using EeveexModManager.Classes.DatabaseClasses;
using EeveexModManager.Classes.JsonClasses.API;
using EeveexModManager.Interfaces;
using EeveexModManager.Nexus;
using EeveexModManager.Controls;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Threading;

namespace EeveexModManager.Classes
{
    public class ModManager
    {
        private DatabaseContext _db;
        private HttpClient httpClient;

        private TreeView Mods_TreeView { get; }
        private TreeView Downloads_TreeView { get; }

        public string CookieSid { get; protected set; } = null;
        public bool CanAccessApi { get; protected set; } = false;

        public DownloadManager DownloadsManager { get; protected set; }

        public ModManager(DatabaseContext db)
        {
            httpClient = new HttpClient();
            _db = db;
        }

        public ModManager(DatabaseContext db, TreeView modsTreeView, TreeView downloadsTreeView) : this(db)
        {
            httpClient = new HttpClient();
            DownloadsManager = new DownloadManager(downloadsTreeView, AddModToGUI);

            Downloads_TreeView = downloadsTreeView;
            Mods_TreeView = modsTreeView;
        }

        public void UpdateCookieSid(string newCookieSid)
        {
            CookieSid = newCookieSid;
            ToggleCanAccessApi();
        }

        public void ToggleCanAccessApi()
        {
            CanAccessApi = !CanAccessApi;
        }

        public void AddModToGUI(Mod m)
        {
            _db.GetCollection<Db_Mod>("mods").Insert(m.EncapsulateToDb());
            Mods_TreeView.Items.Add(new Mod_Control(m, Mods_TreeView.Items.Count, _db));
        }

        [STAThread]
        public void CreateMod(Game CurrentGame, ModArchiveFile archive)
        {
            /*OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Archive Files|*.rar;*.zip;*.7z;"
            };*/
            int modIndex = _db.GetCollection<Db_Mod>("mods").Count();


            Mod newMod = new Mod($"Offline Mod #{modIndex}", archive.FileName, false, true, archive.FullNewPath, archive.ModDirectory, archive.DownloadDirectory,
                CurrentGame.Id, ModCategories.Unknown, $"off-{modIndex}");

            archive.Extract();
            AddModToGUI(newMod);
        }

        
        public async Task CreateMod(Game CurrentGame, NexusUrl nexusUrl)
        {
            if (!_db.GetCollection<Db_Mod>("mods").FindAll().Select(x => x.FileId).Contains(nexusUrl.FileId))
            {
                string API_DownloadLinkSource = $"http://nmm.nexusmods.com/{CurrentGame.Name_API}/Files/download/{nexusUrl.FileId}/?game_id={(int)CurrentGame.Id}";
                string API_ModInfoSource = $"http://nmm.nexusmods.com/{CurrentGame.Name_API}/Mods/{nexusUrl.ModId}/?game_id={(int)CurrentGame.Id}";
                string API_ModFileInfoSource = $"http://nmm.nexusmods.com/{CurrentGame.Name_API}/Files/{nexusUrl.FileId}/?game_id={(int)CurrentGame.Id}";

                httpClient.BaseAddress = new Uri("http://localhost:9000/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Nexus Client v0.63.14");
                if (CanAccessApi)
                    httpClient.DefaultRequestHeaders.Add("Cookie", $"sid={CookieSid}");

                string res1 = (await (await httpClient.GetAsync(API_DownloadLinkSource)).Content.ReadAsStringAsync()).Remove(0, 1);
                res1 = res1.Remove(res1.Length - 1, 1); //removing first and last '[' ']' cus i don't see any reason for it to be a list

                string res2 = (await (await httpClient.GetAsync(API_ModInfoSource)).Content.ReadAsStringAsync()); //gitting mod info

                string res3 = (await (await httpClient.GetAsync(API_ModFileInfoSource)).Content.ReadAsStringAsync()); //gitting mod's file info

                var result_downloadInfo = JsonConvert.DeserializeObject<Api_ModDownloadInfo>(res1);
                var result_modInfo = JsonConvert.DeserializeObject<Api_ModInfo>(res2);
                var result_fileInfo = JsonConvert.DeserializeObject<Api_ModFileInfo>(res3);

                string DownloadTo = $@"{CurrentGame.DownloadsDirectory }\{result_modInfo.name} [{result_fileInfo.name}]";
                string DownloadAs = DownloadTo + $@"\{result_fileInfo.uri}";
                string InstallTo = $@"{CurrentGame.ModsDirectory}\{result_modInfo.name} [{result_fileInfo.name}]";
                if (!Directory.Exists(DownloadTo))
                {
                    Directory.CreateDirectory(DownloadTo);
                }

                if (File.Exists(DownloadAs))
                    File.Delete(DownloadAs);

                Mod newMod = new Mod(result_modInfo.name, result_fileInfo.name, false, true, DownloadAs, InstallTo, DownloadTo,
                    CurrentGame.Id, (ModCategories)result_modInfo.category_id, nexusUrl.FileId, result_modInfo.version, nexusUrl.ModId,
                    result_modInfo.author, nexusUrl.SourceUrl.ToString(), true);
                
                Thread t = new Thread( () =>
                Downloads_TreeView.Dispatcher.Invoke(() =>
                {
                    DownloadsManager.AddDownload(new Uri(result_downloadInfo.URI), DownloadAs, DownloadTo, InstallTo, result_modInfo.name, result_fileInfo.name, newMod);
                }, DispatcherPriority.ApplicationIdle));
                //t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
        }
    }
}
