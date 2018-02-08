using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using EeveexModManager.Interfaces;
using EeveexModManager.Controls;
using System.ComponentModel;
using System.IO;

namespace EeveexModManager.Classes
{
    public class Download : IDownload
    {
        public string ModName { get; }
        public string ModFileName { get; }
        public Uri DownloadFrom { get; }
        public string DownloadTo { get; }
        public string DownloadAt { get; }
        public string InstallAt { get; }
        public WebClient Client { get; }

        public ModDownload_Control AssociatedDownloadControl { get; set; }

        public Download(Uri from, string As, string At, string installAt, string modN, string modFileN)
        {
            Client = new WebClient();
            ModName = modN;
            ModFileName = modFileN;
            DownloadFrom = from;
            DownloadTo = As;
            DownloadAt = At;
            InstallAt = installAt;
        }

        public void StartDownload()
        {
            
            Client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompletedCallback);
            Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
            Client.DownloadFileAsync(DownloadFrom, DownloadTo);
        }

        private void DownloadCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            AssociatedDownloadControl.DownloadFinished(e);
        }

        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            AssociatedDownloadControl.UpdateProgress(e);
        }
    }
}
