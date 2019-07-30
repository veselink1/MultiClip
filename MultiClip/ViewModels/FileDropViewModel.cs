using System;
using MultiClip.Clipboard;
using MultiClip.Network;

namespace MultiClip.ViewModels
{
    public class FileDropViewModel : ClipboardViewModel
    {
        public FileDropViewModel(ClipboardState state, DateTime dateTime, 
            Action<ClipboardViewModel> onPaste, Action<ClipboardViewModel> onDelete) 
                : base(state, dateTime, onPaste, onDelete)
        {
        }

        public string[] Items { get; set; }
    }
}
