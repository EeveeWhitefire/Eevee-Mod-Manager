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
        public ModDownload_Control AssociatedDownloadControl { get; set; }

        public string ModName { get; }
        public string ModFileName { get; }
        public Uri DownloadFrom { get; } //url
        public string DownloadTo { get; } //download path
        public string DownloadAt { get; } //download dir
        public string InstallAt { get; } //install dir

        //Variables
        public long FileSize { get; set; } = 0;
        public long DownloadedSize { get; protected set; } = 0;
        public float ProgressPercentage { get; protected set; } = 0;
        public long DownloadSpeed;
        public long AverageDownloadSpeed;
        public DownloadStates DownloadState { get; protected set; } = DownloadStates.PreStart;


        private List<int> _downloadBytes;
        private List<int> _downloadSpeeds;
        private int _lastIndex = 0;

        private byte[] totalDownloadBuffer;

        private byte[] downloadBuffer;
        private HttpWebResponse webResponse;
        private DateTime _start;
        private BinaryReader binaryReader;

        public Download(Uri from, string As, string At, string installAt, string modN, string modFileN)
        {
            ModName = modN;
            ModFileName = modFileN;
            DownloadFrom = from;
            DownloadTo = As;
            DownloadAt = At;
            InstallAt = installAt;

            _downloadSpeeds = new List<int>(); //for average speed calculation
            _downloadBytes = new List<int>(); //for speed calculation

            _start = DateTime.UtcNow;
        }

        public void ResetProperties()
        {
            DownloadedSize = 0;
            ProgressPercentage = 0;
            Stop();
            GC.Collect();
        }

        int bytesSize = 0; //represents how many bytes have been downloaded per reading
        int lastBytesSize = 0;

        public void Downloading()
        {
            // Download file bytes until the download is paused, canceled or completed
            while (DownloadState != DownloadStates.Canceled && DownloadState != DownloadStates.Finished
            && DownloadState != DownloadStates.Paused)
            {
                try
                {
                    // Read data from the response stream and write it to the buffer
                    bytesSize = binaryReader.Read(downloadBuffer, 0, (int)FileSize);
                    if (bytesSize > 0)
                    {
                        Array.Copy(downloadBuffer, 0, totalDownloadBuffer, lastBytesSize, bytesSize); //copying the content to the full buffer

                        _downloadBytes.Add(bytesSize);
                        DownloadedSize = _downloadBytes.Sum();
                        ProgressPercentage = (int)(((float)DownloadedSize / (float)FileSize) * 100);
                        AssociatedDownloadControl.UpdateProgress();

                        Array.Clear(downloadBuffer, 0, bytesSize); //clearing for future content

                        if (DownloadedSize >= FileSize)
                        {
                            Finished();
                        }
                    }
                    else
                    {
                        Finished();
                    }
                    lastBytesSize += bytesSize;
                }
                catch (Exception)
                {
                    break;
                }
            }

            if (DownloadState == DownloadStates.Finished)
            {
                File.WriteAllBytes(DownloadTo, totalDownloadBuffer);
                Stop();
            }
        }

        public void Finished()
        {
            DownloadState = DownloadStates.Finished;
            ProgressPercentage = 100; //100%
            DownloadedSize = FileSize;
            AssociatedDownloadControl.UpdateProgress();
        }


        public void Start()
        {
            if (DownloadState == DownloadStates.PreStart || DownloadState == DownloadStates.Paused)
            {
                DownloadState = DownloadStates.InProgress;
                StartDownload();
            }
        }
        public void StartDownload()
        {
            HttpWebRequest webRequest = null;
            webResponse = null;
            Stream responseStream = null;
            _downloadSpeeds.Clear();
            Task.Run(() =>
            {
                try
                {
                    // Create request to the server to download the file
                    webRequest = (HttpWebRequest)WebRequest.Create(DownloadFrom);
                    webResponse = (HttpWebResponse)webRequest.GetResponse();
                    responseStream = webResponse.GetResponseStream();
                    binaryReader = new BinaryReader(responseStream, Encoding.UTF8);

                    // Set a 5 second timeout, in case of internet connection break
                    responseStream.ReadTimeout = 5000;
                    FileSize = webResponse.ContentLength;

                    downloadBuffer = new byte[FileSize]; //dunno every speed so will limit it to the file size
                    totalDownloadBuffer = new byte[FileSize];
                    Downloading();
                }
                catch (Exception e)
                {
                    if(DownloadState != DownloadStates.Canceled)
                    {
                        throw e;
                    }
                }
            });
        }

        public void Stop()
        {
            binaryReader?.BaseStream?.Dispose();
            binaryReader?.Dispose();
            binaryReader = null;

            if (downloadBuffer != null)
                Array.Clear(downloadBuffer, 0, downloadBuffer.Length); //clearing for future content
            if(totalDownloadBuffer != null)
                Array.Clear(totalDownloadBuffer, 0, (int)FileSize); //clearing for future content
            downloadBuffer = null;
            totalDownloadBuffer = null;
        }

        #region Features (cancel, pause, restart, resume)
        public void Cancel()
        {
            if (DownloadState == DownloadStates.InProgress)
            {
                DownloadState = DownloadStates.Canceled;
                ResetProperties();
                AssociatedDownloadControl.UpdateProgress();
            }
        }
        // Restart download
        public void Restart()
        {
            if (DownloadState == DownloadStates.Canceled || DownloadState == DownloadStates.Finished)
            {
                ResetProperties();
                if (File.Exists(DownloadTo))
                {
                    File.Delete(DownloadTo);
                }
                DownloadState = DownloadStates.PreStart;
                AssociatedDownloadControl.UpdateProgress();
                Task.Run(() => AssociatedDownloadControl.AssociatedView.Dispatcher.Invoke(() =>
                {
                    Start();
                }));
            }
        }

        // Pause download
        public void Pause()
        {
            if (DownloadState == DownloadStates.InProgress)
            {
                DownloadState = DownloadStates.Paused;
                AssociatedDownloadControl.UpdateProgress();
            }
        }
        public void Resume()
        {
            if (DownloadState == DownloadStates.Paused)
            {
                DownloadState = DownloadStates.InProgress;
                Task.Run(() => Downloading());
            }
        }
        #endregion

        public Task CalculateDownloadSpeed()
        {
            if (DownloadState == DownloadStates.InProgress && (DateTime.UtcNow - _start).TotalSeconds >= 1)
            {
                int startIndex = _lastIndex;
                Task.Delay(1000);

                int endIndex = _downloadBytes.Count;

                var bytesThisSecond = new List<int>();

                bytesThisSecond.AddRange(_downloadBytes);
                bytesThisSecond = bytesThisSecond.Skip(_lastIndex).Take(endIndex - startIndex).ToList();
                int sum = bytesThisSecond.Sum();
                if (bytesThisSecond.Count > 0)
                {
                    int speed = sum / bytesThisSecond.Count;
                    _downloadSpeeds.Add(speed);
                    DownloadSpeed = speed / 1000; //KB
                    AverageDownloadSpeed = (_downloadSpeeds.Sum() / _downloadSpeeds.Count) / 1000;
                }
                else
                    DownloadSpeed = 0;
                _lastIndex = endIndex - startIndex;
                //AssociatedDownloadControl.AssociatedView.Items.Refresh();
                _start = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }
    }
}
