using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DylanDashboard.Anime
{
    public class DownloadManager
    {
        private static DownloadManager? _instance;

        private bool _isPolling;

        public event EventHandler<TorrentListUpdateEventArgs>? TorrentListUpdated;

        public event EventHandler<DownloadFilesUpdateEventArgs>? DownloadFilesUpdated;

        public event EventHandler<AddTorrentsErrorEventArgs>? AddTorrentsErrorEvent;

        private DownloadManager() {
            _isPolling = false;
        }

        public static DownloadManager GetIntance()
        {
            if (_instance == null)
            {
                _instance = new DownloadManager();
            }
            return _instance;
        }

        public void StartPolling()
        {
            if (_isPolling)
            {
                return;
            }
            _isPolling = true;
            PollTorrentListAsync();
            PollDownloadFilesAsync();
        }

        private async void PollTorrentListAsync()
        {
            var listProcessInfo = new ProcessStartInfo("transmission-remote", "--list")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var listProcess = Utils.StartProcess(listProcessInfo);
            if (listProcess == null)
            {
                TorrentListUpdated?.Invoke(this, new TorrentListUpdateEventArgs()
                {
                    Error = $"Failed to create process for: { listProcessInfo.FileName } { listProcessInfo.Arguments }",
                    Torrents = null
                });
                await Task.Delay(10000);
                PollTorrentListAsync();
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
                    var completePercentage = 0;
                    try
                    {
                        completePercentage = Convert.ToInt32(torrentData[1].Split('%')[0]);
                    } catch { }
                    torrents.Add(new Torrent()
                    {
                        Id = Convert.ToInt32(torrentData[0]),
                        CompletePercentage = completePercentage,
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

            foreach (var torrent in torrents)
            {
                if (torrent.Status == "Seeding")
                {
                    var removeProcess = Utils.StartProcess(new ProcessStartInfo("transmission-remote", $"-t { torrent.Id } --remove")
                    {
                        CreateNoWindow = true
                    });
                    if (removeProcess != null)
                    {
                        await removeProcess.WaitForExitAsync();
                    }
                }
            }

            if (listProcess.ExitCode != 0)
            {
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
            PollTorrentListAsync();
        }

        private async void PollDownloadFilesAsync()
        {
            var files = Directory.GetFiles($"{ Environment.GetEnvironmentVariable("HOMEPATH") }\\Downloads", "*.mkv", new EnumerationOptions()
            {
                RecurseSubdirectories = true
            });

            var videoFiles = new List<VideoFile>();
            foreach (var file in files)
            {
                videoFiles.Add(new VideoFile()
                {
                    FilePath = file
                });
            }

            DownloadFilesUpdated?.Invoke(this, new DownloadFilesUpdateEventArgs()
            {
                VideoFiles = videoFiles
            });

            await Task.Delay(10000);
            PollDownloadFilesAsync();
        }

        public async void AddTorrentsAsync(List<string> torrents)
        {
            var errors = new List<string>();
            foreach (var torrent in torrents)
            {
                var addTorrentInfo = new ProcessStartInfo("transmission-remote", $"--add \"{ torrent }\"")
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                var addTorrentProcess = Utils.StartProcess(addTorrentInfo);
                if (addTorrentProcess == null)
                {
                    errors.Add($"Failed to create process for: { addTorrentInfo.FileName } { addTorrentInfo.Arguments }");
                    continue;
                }
                var errorOutput = await addTorrentProcess.StandardOutput.ReadToEndAsync();
                if (addTorrentProcess.ExitCode != 0)
                {
                    errors.Add(errorOutput.Trim());
                }
            }
            if (errors.Count > 0)
            {
                AddTorrentsErrorEvent?.Invoke(this, new AddTorrentsErrorEventArgs()
                {
                    Error = string.Join(Environment.NewLine, errors)
                });
            }
        }
    }

    public class TorrentListUpdateEventArgs : EventArgs
    {   
        public string? Error { get; set; }
        public List<Torrent>? Torrents { get; set; }
    }

    public class DownloadFilesUpdateEventArgs : EventArgs
    {
        public List<VideoFile>? VideoFiles { get; set; }
    }

    public class AddTorrentsErrorEventArgs : EventArgs
    {
        public string? Error { get; set; }
    }
}
