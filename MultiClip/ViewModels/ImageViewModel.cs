using System;
using System.Windows.Media.Imaging;
using MultiClip.Clipboard;
using MultiClip.Network;

namespace MultiClip.ViewModels
{
    public class ImageViewModel : ClipboardViewModel
    {
        public ImageViewModel(ClipboardState state, DateTime dateTime, 
            Action<ClipboardViewModel> onPaste, Action<ClipboardViewModel> onDelete) 
                : base(state, dateTime, onPaste, onDelete)
        {
        }

        public BitmapFrame Image { get; set; }
    }
}
