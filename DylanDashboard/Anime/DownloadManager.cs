using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DylanDashboard.Anime
{
    public class DownloadManager
    {
        private static DownloadManager _instance;

        public event EventHandler<TorrentListUpdateEventArgs> TorrentListUpdated;

        private DownloadManager() {
            GetTorrentList();
        }

        public static DownloadManager GetIntance()
        {
            if (_instance == null)
            {
                _instance = new DownloadManager();
            }
            return _instance;
        }

        private async void GetTorrentList()
        {
            var listProcessInfo = new ProcessStartInfo("transmission-remote", "localhost:9091 --list")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var listProcess = Process.Start(listProcessInfo);
            if (listProcess == null)
            {
                TorrentListUpdated?.Invoke(this, new TorrentListUpdateEventArgs()
                {
                    Error = $"Failed to create process for: { listProcessInfo.FileName } { listProcessInfo.Arguments }",
                    Torrents = null
                });
                await Task.Delay(10000);
                GetTorrentList();
                return;
            }

            var error = "";
            var errorLine = await listProcess.StandardError.ReadLineAsync();
            var torrents = new List<Torrent>();
            var outLine = await listProcess.StandardOutput.ReadLineAsync();
            var headerRegex = new Regex(@"^\s+ID.*$");
            var sumRegex = new Regex(@"^Sum.*$");
            while (outLine != null || errorLine != null)
            {
                if (outLine != null && !headerRegex.IsMatch(outLine) && !sumRegex.IsMatch(outLine))
                {
                    var torrentData = Regex.Split(outLine.Trim(), @"\s\s+");
                    Debug.WriteLine($"GetTorrentList: { outLine }");
                    torrents.Add(new Torrent()
                    {
                        Id = Convert.ToInt32(torrentData[0]),
                        CompletePercentage = Convert.ToInt32(torrentData[1].Split('%')[0]),
                        Status = torrentData[7],
                        Title = torrentData[8]
                    });
                }
                else
                {
                    error += $"{ (error.Length > 0 ? Environment.NewLine : "") }{ errorLine }";
                }
                errorLine = await listProcess.StandardError.ReadLineAsync();
                outLine = await listProcess.StandardOutput.ReadLineAsync();
            }

            if (listProcess.ExitCode != 0)
            {
                Debug.WriteLine($"GetTorrentListError: { error }");
                TorrentListUpdated?.Invoke(this, new TorrentListUpdateEventArgs()
                {
                    Error = error,
                    Torrents = null
                });
            }
            else
            {
                TorrentListUpdated?.Invoke(this, new TorrentListUpdateEventArgs()
                {
                    Error = "",
                    Torrents = torrents
                });
            }

            await Task.Delay(10000);
            GetTorrentList();
        }
    }

    public class TorrentListUpdateEventArgs : EventArgs
    {   
        public string Error { get; set; }
        public List<Torrent> Torrents { get; set; }
    }
}
