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

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPageViewModel()
        {
            DownloadManager.GetIntance().TorrentListUpdated += OnTorrentListUpdated;
        }

        private void OnTorrentListUpdated(object sender, TorrentListUpdateEventArgs e)
        {
            TorrentListError = e.Error;
            TorrentList = e.Torrents;
        }

        private void NotifyPropertyChanged(string prorpertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prorpertyName));
        }
    }
}
