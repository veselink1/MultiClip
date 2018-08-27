using System;
using System.Windows.Media.Imaging;
using MultiClip.Clipboard;
using MultiClip.Network;

namespace MultiClip.ViewModels
{
    public class ImageViewModel : ClipboardViewModel
    {
        public ImageViewModel(ClipboardState state, DateTime dateTime, 
            Host optionalHost, Action<ClipboardViewModel> onPaste, Action<ClipboardViewModel> onDelete) 
                : base(state, dateTime, optionalHost, onPaste, onDelete)
        {
        }

        public BitmapFrame Image { get; set; }
    }
}
