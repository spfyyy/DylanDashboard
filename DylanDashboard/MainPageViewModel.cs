using DylanDashboard.Anime;
using System.ComponentModel;

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

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainPageViewModel()
        {
            var dm = DownloadManager.GetIntance();
            dm.TorrentListUpdated += OnTorrentListUpdated;
            dm.DownloadFilesUpdated += OnDownloadFilesUpdated;
            dm.StartPolling();
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
