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
using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Controls
{
    public class ModDownload_Control : Control
    {
        public ListView AssociatedView { get; }
        public Download AssociatedDownload { get; }
        public Mod AssociatedMod { get; }

        public Action<Mod> AddToGUI { get; }

        //Formatted
        public string DownloadName { get; set; }
        public string CurrentNumberOfBytes { get; set; }
        public string NumberOfBytesInTotal { get; set; }
        public string DownloadedPercentage { get; set; }
        public string ModId { get; set; }
        public string State { get; set; }
        public string Speed { get; set; }
        public string AverageSpeed { get; set; }

        public ModDownload_Control(ListView parent, Download dl, Action<Mod> addToGui, Mod m) : base()
        {
            AssociatedView = parent;
            AssociatedDownload = dl;
            AddToGUI = addToGui;
            AssociatedMod = m;

            DownloadName = $"{AssociatedDownload.ModName} [{AssociatedDownload.ModFileName}]";
            CurrentNumberOfBytes = "0 KB";
            NumberOfBytesInTotal = "0 KB";

            DownloadedPercentage = "0 %";
            State = "Starting";
        }

        public void UpdateProgress()
        {
            AssociatedDownload.CalculateDownloadSpeed();

            CurrentNumberOfBytes = $"{AssociatedDownload.DownloadedSize / 1000} KB";
            NumberOfBytesInTotal = $"{AssociatedDownload.FileSize / 1000} KB";
            DownloadedPercentage = $"{AssociatedDownload.ProgressPercentage} %";
            Speed = $"{AssociatedDownload.DownloadSpeed} KB\\s";
            AverageSpeed = $"{AssociatedDownload.AverageDownloadSpeed} KB\\s";
            switch (AssociatedDownload.DownloadState)
            {
                case Interfaces.DownloadStates.PreStart:
                    State = "Started";
                    break;
                case Interfaces.DownloadStates.InProgress:
                    State = "Downloading";
                    break;
                case Interfaces.DownloadStates.Paused:
                    State = "Paused";
                    break;
                case Interfaces.DownloadStates.Finished:
                    State = "Complete!";
                    break;
                case Interfaces.DownloadStates.Canceled:
                    State = "Canceled";
                    break;
                default:
                    break;
            }
            Task.Run( () => AssociatedView.Dispatcher.Invoke(() => AssociatedView.Items.Refresh()));
        }

        public void CancelDownload()
        {
            Task.Run(() =>
            {
               AssociatedDownload.Cancel();

               FileInfo fileInfo = new FileInfo(AssociatedDownload.DownloadTo);
               while (IsFileLocked(fileInfo))
               {
                   Task.Delay(1000);
               }
               if (Directory.Exists(AssociatedDownload.DownloadAt))
                   Directory.Delete(AssociatedDownload.DownloadAt, true);
            });
        }

        public void PauseDownload()
        {
            Task.Run(() =>
            {
                AssociatedDownload.Pause();
            });
        }

        public void ResumeDownload()
        {
            Task.Run(() =>
            {
                AssociatedDownload.Resume();
            });
        }

        public void RestartDownload()
        {
            Task.Run(() =>
            {
                AssociatedDownload.Restart();
            });
        }

        public void InstallMod()
        {
            try
            {
                Task.Run(() =>
                {
                    ModArchiveFile archive = new ModArchiveFile(AssociatedDownload.DownloadTo, AssociatedDownload.InstallAt);
                    SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    NTAccount act = sid.Translate(typeof(NTAccount)) as NTAccount;

                    FileSecurity sec = File.GetAccessControl(AssociatedDownload.DownloadTo);

                    sec.AddAccessRule(new FileSystemAccessRule(act, FileSystemRights.FullControl, AccessControlType.Allow));
                    File.SetAccessControl(AssociatedDownload.DownloadTo, sec);

                    archive.Extract();
                    AssociatedView.Dispatcher.Invoke(() =>
                    {
                        State = "Installed!";
                        AssociatedView.Items.Refresh();
                        AddToGUI(AssociatedMod);
                    });
                });
            }
            catch (Exception e)
            {
            }
            
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


