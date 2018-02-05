using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using EeveexModManager.Controls;

namespace EeveexModManager.Classes
{
    public class DownloadManager
    {
        private TreeView ViewControl { get; }
        private List<Download> Downloads { get; }
        private Action<Mod> AddToGUI { get; }

        public DownloadManager(TreeView view, Action<Mod> addToGui)
        {
            ViewControl = view;
            AddToGUI = addToGui;
            Downloads = new List<Download>();
        }

        public void AddDownload(Uri from, string As, string At, string installAt, string modN, string modFileN, Mod m)
        {
            Download newDl = new Download(from, As, At, installAt, modN, modFileN);
            ModDownload_Control control = new ModDownload_Control(ViewControl, newDl, AddToGUI, m);
            newDl.AssociatedDownloadControl = control;
            
            ViewControl.Items.Add(control);

            newDl.StartDownload();
        }
    }
}
