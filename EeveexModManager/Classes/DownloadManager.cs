using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

using EeveexModManager.Controls;

namespace EeveexModManager.Classes
{
    public class DownloadManager
    {
        private ListView ViewControl { get; }
        private List<Download> Downloads { get; }
        public List<ModDownload_Control> DownloadControls { get; protected set; }
        private Action<Mod> AddToGUI { get; }

        public DownloadManager(ListView view, Action<Mod> addToGui)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 10;
            ViewControl = view;
            AddToGUI = addToGui;
            Downloads = new List<Download>();
            DownloadControls = new List<ModDownload_Control>();
            ViewControl.ItemsSource = DownloadControls;
        }

        public async Task AddDownload(Uri from, string As, string At, string installAt, string modN, string modFileN, Mod m)
        {
            Download newDl = new Download(from, As, At, installAt, modN, modFileN);
            ModDownload_Control control = new ModDownload_Control(ViewControl, newDl, AddToGUI, m);
            newDl.AssociatedDownloadControl = control;

            Downloads.Add(newDl);
            DownloadControls.Add(control);
            await ViewControl.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
            {
                ViewControl.Items.Refresh();
                newDl.Start();
            }));
        }
    }
}
