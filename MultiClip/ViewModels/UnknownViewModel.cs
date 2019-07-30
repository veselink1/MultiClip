using System;
using MultiClip.Clipboard;
using MultiClip.Network;

namespace MultiClip.ViewModels
{
    public class UnknownViewModel : ClipboardViewModel
    {
        public UnknownViewModel(ClipboardState state, DateTime dateTime, 
            Action<ClipboardViewModel> onPaste, Action<ClipboardViewModel> onDelete) 
                : base(state, dateTime, onPaste, onDelete)
        {
        }
    }
}
