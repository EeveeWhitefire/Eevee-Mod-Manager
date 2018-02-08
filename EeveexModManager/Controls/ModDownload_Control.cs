using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EeveexModManager.Classes;

namespace EeveexModManager.Controls
{
    public class ModDownload_Control : StackPanel
    {
        public TreeView AssociatedDownloadsView { get; }
        public Download AssociatedDownload { get; }
        public Action<Mod> AddToGUI { get; }

        private Mod AssoicatedMod { get; }

        private TextBlock DownloadInfo;
        private TextBlock CurrentNumberOfBytes;
        private TextBlock NumberOfBytesInTotal;
        private TextBlock DownloadedPercentage;
        private TextBlock State;
        private Button CancelDownloadButton;

        public ModDownload_Control(TreeView parent, Download dl, Action<Mod> addToGui, Mod m) : base()
        {
            AssociatedDownloadsView = parent;
            AssociatedDownload = dl;
            AddToGUI = addToGui;
            AssoicatedMod = m;

            Orientation = Orientation.Horizontal;
            Thickness ListMargin = new Thickness(10, 2, 10, 0);

            DownloadInfo = new TextBlock()
            {
                Text = $"{AssociatedDownload.ModName} [{AssociatedDownload.ModFileName}]",
                Margin = new Thickness(0, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Width = 350,
                FontSize = 15
            };
            CurrentNumberOfBytes = new TextBlock()
            {
                Text = "Downloaded: 0 KB",
                Margin = ListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200,
                FontSize = 15
            };
            NumberOfBytesInTotal = new TextBlock()
            {
                Text = "Out of: 0 KB",
                Margin = ListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 180,
                FontSize = 15
            };
            DownloadedPercentage = new TextBlock()
            {
                Text = "Percentage: 0 %",
                Margin = ListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 125,
                FontSize = 15
            };
            State = new TextBlock()
            {
                Text = "State: Starting",
                Margin = ListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 130,
                FontSize = 15
            };


            CancelDownloadButton = new Button()
            {
                Content = "X",
                Background = Brushes.DarkRed,
                Foreground = Brushes.White,
                Margin = ListMargin,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 20,
                Height = 25,
                FontSize = 15
            };
            CancelDownloadButton.Click += new RoutedEventHandler(CancelDownloadButton_Click);

            Children.Add(CancelDownloadButton);
            Children.Add(DownloadInfo);
            Children.Add(CurrentNumberOfBytes);
            Children.Add(NumberOfBytesInTotal);
            Children.Add(DownloadedPercentage);
            Children.Add(State);
        }

        public void UpdateProgress(DownloadProgressChangedEventArgs e)
        {
            CurrentNumberOfBytes.Text = $"Downloaded: {e.BytesReceived / 1000} KB";
            NumberOfBytesInTotal.Text = $"Out of: {e.TotalBytesToReceive / 1000} KB";
            DownloadedPercentage.Text = $"Percentage: {e.ProgressPercentage} %";
            State.Text = "State: Downloading";
        }

        public void DownloadFinished(AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                AssociatedDownload.StartDownload();
            }
            else if(!e.Cancelled)
            {
                if (DownloadedPercentage.Text.Contains("100"))
                {
                    State.Text = "State: Done";
                    CancelDownloadButton.IsEnabled = false;
                    CancelDownloadButton.Background = Brushes.Gray;

                    AddToGUI(AssoicatedMod);

                    ModArchiveFile archive = new ModArchiveFile(AssociatedDownload.DownloadTo, AssociatedDownload.InstallAt);
                    SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    NTAccount act = sid.Translate(typeof(NTAccount)) as NTAccount;

                    FileSecurity sec = File.GetAccessControl(AssociatedDownload.DownloadTo);

                    sec.AddAccessRule(new FileSystemAccessRule(act, FileSystemRights.FullControl, AccessControlType.Allow));
                    File.SetAccessControl(AssociatedDownload.DownloadTo, sec);

                    archive.Extract();
                }
                AssociatedDownload.Client.CancelAsync();
                AssociatedDownload.Client.Dispose();
            }
        }

        private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            State.Text = "State: Canceled";
            AssociatedDownload.Client.CancelAsync();
            AssociatedDownload.Client.Dispose();
            GC.Collect();
            CancelDownloadButton.IsEnabled = false;
            CancelDownloadButton.Background = Brushes.Gray;

            FileInfo fileInfo = new FileInfo(AssociatedDownload.DownloadTo);
            while(IsFileLocked(fileInfo))
            {
                Task.Delay(1000);
            }
            if (Directory.Exists(AssociatedDownload.DownloadAt))
                Directory.Delete(AssociatedDownload.DownloadAt, true);

        }

        bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Flush();
                    stream.Dispose();
                    stream.Close();
                }
            }

            //file is not locked
            return false;
        }
    }
}
