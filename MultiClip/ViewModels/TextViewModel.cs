using System;
using MultiClip.Clipboard;
using MultiClip.Network;

namespace MultiClip.ViewModels
{
    public class TextViewModel : ClipboardViewModel
    {
        public TextViewModel(ClipboardState state, DateTime dateTime, 
            Host optionalHost, Action<ClipboardViewModel> onPaste, Action<ClipboardViewModel> onDelete) 
                : base(state, dateTime, optionalHost, onPaste, onDelete)
        {
        }

        public string Text { get; set; }
    }
}
