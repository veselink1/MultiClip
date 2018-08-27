using System.Collections.ObjectModel;
using MultiClip.Clipboard;
using MultiClip.Network;

namespace MultiClip.Models
{
    public class RemoteClipboardState
    {
        public Host Identity { get; set; }
        public ObservableCollection<ClipboardState> States { get; set; }
    }
}
