using DylanDashboard.Anime;
using System.Diagnostics;

namespace DylanDashboard;

public partial class MainPage : ContentPage
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
                OnPropertyChanged(nameof(TorrentListError));
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
                OnPropertyChanged(nameof(TorrentList));
            }
        }
    }

    public MainPage()
	{
		InitializeComponent();
        DownloadManager.GetIntance().TorrentListUpdated += MainPage_TorrentListUpdated;
	}

    private void MainPage_TorrentListUpdated(object sender, TorrentListUpdateEventArgs e)
    {
        TorrentListError = e.Error;
        if (e.Torrents != null)
        {
            foreach (var torrent in e.Torrents)
            {
                Debug.WriteLine($"MainPainTorrent: { torrent.Title }: { torrent.CompletePercentage }% Downloaded: { torrent.Status }");
            }
            TorrentList = e.Torrents;
        }
    }
}
