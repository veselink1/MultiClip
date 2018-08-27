using System.Collections.ObjectModel;
using MultiClip.Utilities;

namespace MultiClip.ViewModels
{
    public class OverviewViewModel : ObservableObject
    {
        private ObservableCollection<ClipboardViewModel> _localClipboards = new ObservableCollection<ClipboardViewModel>();
        private ObservableCollection<ClipboardViewModel> _remoteClipboards = new ObservableCollection<ClipboardViewModel>();
        private ObservableCollection<RemoteHostViewModel> _remoteHosts = new ObservableCollection<RemoteHostViewModel>();

        public ObservableCollection<ClipboardViewModel> LocalClipboards
        {
            get => _localClipboards;
            set { _localClipboards = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<ClipboardViewModel> RemoteClipboards
        {
            get => _remoteClipboards;
            set { _remoteClipboards = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<RemoteHostViewModel> RemoteHosts
        {
            get => _remoteHosts;
            set { _remoteHosts = value; NotifyPropertyChanged(); }
        }
    }
}
