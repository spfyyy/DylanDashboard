using DylanDashboard.Anime;
using System.ComponentModel;
using System.Windows.Input;

namespace DylanDashboard
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private string _torrentListError = "";
        public string TorrentListError
        {
            get
            {
                return _torrentListError;
            }
            set
            {
                if (_torrentListError != value)
                {
                    _torrentListError = value;
                    NotifyPropertyChanged(nameof(TorrentListError));
                }
            }
        }

        private List<Torrent> _torrentList = new();
        public List<Torrent> TorrentList
        {
            get
            {
                return _torrentList;
            }
            set
            {
                if (value != null)
                {
                    _torrentList = value;
                    NotifyPropertyChanged(nameof(TorrentList));
                }
            }
        }

        private string _torrentAddError = "";
        public string TorrentAddError
        {
            get
            {
                return _torrentAddError;
            }
            set
            {
                _torrentAddError = value;
                NotifyPropertyChanged(nameof(TorrentAddError));
            }
        }

        private string _torrentAddText = "";
        public string TorrentAddText
        {
            get
            {
                return _torrentAddText;
            }
            set
            {
                _torrentAddText = value;
                if (AddTorrentCommand is Command command)
                {
                    command.ChangeCanExecute();
                }
            }
        }

        private List<VideoFile> _videoFiles = new();
        public List<VideoFile> VideoFiles
        {
            get
            {
                return _videoFiles;
            }
            set
            {
                _videoFiles = value;
                _selectedVideoFile = _videoFiles.Find(vf => _selectedVideoFile != null && _selectedVideoFile.FilePath == vf.FilePath);
                NotifyPropertyChanged(nameof(VideoFiles));
                NotifyPropertyChanged(nameof(SelectedVideoFile));
            }
        }

        private VideoFile? _selectedVideoFile;
        public VideoFile? SelectedVideoFile
        {
            get
            {
                return _selectedVideoFile;
            }
            set
            {
                if (value != null)
                {
                    _selectedVideoFile = value;
                }
            }
        }

        public ICommand AddTorrentCommand { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainPageViewModel()
        {
            var dm = DownloadManager.GetIntance();

            AddTorrentCommand = new Command(() =>
            {
                TorrentAddError = "";
                dm.AddTorrentsAsync(TorrentAddText.Trim().Split("\r").ToList());
                TorrentAddText = "";
                NotifyPropertyChanged(nameof(TorrentAddText));
            }, () =>
            {
                return TorrentAddText.Length > 0;
            });

            dm.TorrentListUpdated += OnTorrentListUpdated;
            dm.DownloadFilesUpdated += OnDownloadFilesUpdated;
            dm.AddTorrentsErrorEvent += OnAddTorrentsErrorEvent;
            dm.StartPolling();
        }

        private void OnAddTorrentsErrorEvent(object? sender, AddTorrentsErrorEventArgs e)
        {
            TorrentAddError = e.Error ?? "";
        }

        private void OnDownloadFilesUpdated(object? sender, DownloadFilesUpdateEventArgs e)
        {
            VideoFiles = e.VideoFiles ?? new();
        }

        private void OnTorrentListUpdated(object? sender, TorrentListUpdateEventArgs e)
        {
            TorrentListError = e.Error ?? "";
            TorrentList = e.Torrents ?? new();
        }

        private void NotifyPropertyChanged(string prorpertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prorpertyName));
        }
    }
}
