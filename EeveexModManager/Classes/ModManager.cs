#define GAMES 

using EeveexModManager.Classes.JsonClasses.API;
using EeveexModManager.Interfaces;
using EeveexModManager.Nexus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Newtonsoft.Json;
using SevenZip;
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


namespace EeveexModManager.Classes
{
    public class ModManager
    {
        private DatabaseContext _db;
        private HttpClient httpClient;
        private WebClient webClient;
        TreeView Mods_TreeView;
        Thickness modListMargin;

        public ModManager(ref DatabaseContext db)
        {
            httpClient = new HttpClient();
            webClient = new WebClient();
            _db = db;
            modListMargin = new Thickness(10, 2, 10, 2);
        }

        public ModManager(ref DatabaseContext db, ref TreeView modsTreeView) : this(ref db)
        {
            httpClient = new HttpClient();
            webClient = new WebClient();
            Mods_TreeView = modsTreeView;
        }


        void AddModToCategory(BaseMod mod, int modIndex, ref TreeViewItem categoryTreeItem)
        {
            int ModNameWidth = 200;

            var x = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            Button newButton = new Button()
            {
                Background = mod.Active ? Brushes.Green : Brushes.Red,
                Content = modIndex,
                Margin = modListMargin,
                Foreground = Brushes.White,
                Width = 15
            };
            newButton.Click += new RoutedEventHandler(ActivateMod);

            x.Children.Add(newButton);
            x.Children.Add(new TextBlock()
            {
                Text = mod.Name,
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = ModNameWidth
            });
            x.Children.Add(new TextBlock()
            {
                Text = mod.Id.ToString(),
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 50
            });
            x.Children.Add(new TextBlock()
            {
                Text = mod.Version,
                Margin = modListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 30
            });

            categoryTreeItem.Items.Add(x);
        }

        private void ActivateMod(object sender, RoutedEventArgs e)
        {

            Button button = (Button)sender;

            if (button.Background == Brushes.Green)
            {
                button.Background = Brushes.Red;
            }
            else
            {
                button.Background = Brushes.Green;
            }

            var mod = _db.GetCollection<BaseMod>("offline_mods").FindAll().ToList()[_db.GetCollection<OnlineMod>("online_mods").Count() - Convert.ToInt32(button.Content)];
            if (mod != null)
            {

                _db.GetCollection<BaseMod>("offline_mods").Update(mod);
            }
            else
            {
                mod = _db.GetCollection<OnlineMod>("online_mods").FindById(Convert.ToInt32(button.Content));
                if (mod != null)
                {
                    mod.Set_isActive(!mod.Active);
                    _db.GetCollection<BaseMod>("offline_mods").Update(mod);
                }
                else
                    MessageBox.Show("Error! No Mod associated");
            }
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
                var CategoryTreeItem = (TreeViewItem)(Mods_TreeView.Items.GetItemAt((int)newMod.ModCategory));
                AddModToCategory(newMod, _db.GetCollection<BaseMod>("offline_mods").Count(), ref CategoryTreeItem);
            }

            _db.GetCollection<BaseMod>("offline_mods").Insert(newMod);
            
        }

        public async Task CreateMod(Game CurrentGame, NexusUrl nexusUrl, bool GUI = true)
        {
            if (!_db.GetCollection<BaseMod>("offline_mods").FindAll().Select(x => x.FileId).Contains(nexusUrl.FileId) &&
                !_db.GetCollection<OnlineMod>("online_mods").FindAll().Select(x => x.FileId).Contains(nexusUrl.FileId))
            {
                string API_DownloadLinkSource = $"http://nmm.nexusmods.com/{CurrentGame.Name_API}/Files/download/{nexusUrl.FileId}/?game_id={(int)CurrentGame.Id}";
                string API_ModInfoSource = $"http://nmm.nexusmods.com/{CurrentGame.Name_API}/Mods/{nexusUrl.ModId}";

                httpClient.BaseAddress = new Uri("http://localhost:9000/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                //httpClient.DefaultRequestHeaders.Add("Cookie", "sid = zCHCHwtHzAttvuHwyDwGDEtGwGDAzxGuBDDHHAxw");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Nexus Client v0.55.8");
                /*
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                httpClient.DefaultRequestHeaders.Add("Host", "nmm.nexusmods.com");*/


                var result_downloadInfo = JsonConvert.DeserializeObject<Api_ModDownloadInfo>(await
                    (await httpClient.GetAsync(API_DownloadLinkSource)).Content.ReadAsStringAsync());

                var result_modInfo = JsonConvert.DeserializeObject<Api_ModInfo>(await
                    (await httpClient.GetAsync(API_ModInfoSource)).Content.ReadAsStringAsync());

                string fileName = result_downloadInfo.URI.AbsolutePath.Replace($"\\{(int)CurrentGame.Id}\\", string.Empty)
                    .Replace('%', ' ');

                string DownloadTo = $@"Mods\{CurrentGame.Name}\{fileName}";

                ModArchiveFile archive = new ModArchiveFile(CurrentGame, DownloadTo);

                webClient.DownloadFileAsync(result_downloadInfo.URI, DownloadTo);


                OnlineMod newMod = new OnlineMod(archive.FileName, false, true, archive.FullNewPath, archive.ModDirectory,
                    CurrentGame.Id, result_modInfo.category, nexusUrl.FileId, result_modInfo.version, nexusUrl.ModId);

                _db.GetCollection<OnlineMod>("online_mods").Insert(newMod);
                

                if (GUI)
                {
                    var CategoryTreeItem = (TreeViewItem)(Mods_TreeView.Items.GetItemAt((int)newMod.ModCategory));
                    AddModToCategory(newMod, _db.GetCollection<BaseMod>("offline_mods").Count(), ref CategoryTreeItem);
                }
            }
        }
    }
}
