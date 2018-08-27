using System;
using MultiClip.Clipboard;
using MultiClip.Network;

namespace MultiClip.ViewModels
{
    public class FileDropViewModel : ClipboardViewModel
    {
        public FileDropViewModel(ClipboardState state, DateTime dateTime, 
            Host optionalHost, Action<ClipboardViewModel> onPaste, Action<ClipboardViewModel> onDelete) 
                : base(state, dateTime, optionalHost, onPaste, onDelete)
        {
        }

        public string[] Items { get; set; }
    }
}
