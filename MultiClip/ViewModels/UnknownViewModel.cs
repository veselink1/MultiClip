using System;
using MultiClip.Clipboard;
using MultiClip.Network;

namespace MultiClip.ViewModels
{
    public class UnknownViewModel : ClipboardViewModel
    {
        public UnknownViewModel(ClipboardState state, DateTime dateTime, 
            Host optionalHost, Action<ClipboardViewModel> onPaste, Action<ClipboardViewModel> onDelete) 
                : base(state, dateTime, optionalHost, onPaste, onDelete)
        {
        }
    }
}
