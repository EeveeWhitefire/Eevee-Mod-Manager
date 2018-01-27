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


namespace EeveexModManager.Classes
{
    public class ModManager
    {
        private DatabaseContext _db;
        private HttpClient httpClient;
        private WebClient webClient;
        TreeView Mods_TreeView;
        public string CookieSid { get; protected set; } = null;
        public bool CanAccessApi { get; protected set; } = false;

        public ModManager(DatabaseContext db)
        {
            httpClient = new HttpClient();
            webClient = new WebClient();
            _db = db;
        }

        public ModManager(DatabaseContext db, TreeView modsTreeView) : this(db)
        {
            httpClient = new HttpClient();
            webClient = new WebClient();
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
        
        public void CreateMod(Game CurrentGame, ModArchiveFile archive, bool GUI = true)
        {
            /*OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Archive Files|*.rar;*.zip;*.7z;"
            };*/
            BaseMod newMod = new BaseMod(archive.FileName, false, true, archive.FullNewPath, archive.ModDirectory,
                CurrentGame.Id, ModCategories.Unknown, $"off-{_db.GetCollection<BaseMod>("offline_mods").Count()}");


            if (GUI)
            {
                Mods_TreeView.Items.Add(new Mod_Control(newMod, Mods_TreeView.Items.Count, _db));
            }

            _db.GetCollection<Db_BaseMod>("offline_mods").Insert(newMod.EncapsulateToDb());
            
        }

        public async Task CreateMod(Game CurrentGame, NexusUrl nexusUrl, bool GUI = true)
        {
            if (!_db.GetCollection<BaseMod>("offline_mods").FindAll().Select(x => x.FileId).Contains(nexusUrl.FileId) &&
                !_db.GetCollection<Db_OnlineMod>("online_mods").FindAll().Select(x => x.FileId).Contains(nexusUrl.FileId))
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

                string DownloadTo = $@"{CurrentGame.DownloadsDirectory }\{result_fileInfo.uri}";

                webClient.DownloadFileAsync((new Uri(result_downloadInfo.URI)), DownloadTo);

                ModArchiveFile archive = new ModArchiveFile(CurrentGame, DownloadTo, result_fileInfo.name);

                OnlineMod newMod = new OnlineMod(result_fileInfo.name, false, false, archive.FullNewPath, archive.ModDirectory,
                    CurrentGame.Id, (ModCategories)result_modInfo.category_id, nexusUrl.FileId, result_modInfo.version, nexusUrl.ModId, nexusUrl.SourceUrl.ToString(), result_modInfo.author);

                _db.GetCollection<Db_OnlineMod>("online_mods").Insert(newMod.EncapsulateToDb_Online());


                if (GUI)
                {
                    Mods_TreeView.Items.Add(new OnlineMod_Control(newMod, Mods_TreeView.Items.Count, _db));
                }
            }
        }
    }
}
