﻿using System;
using System.Windows.Media;
using MultiClip.Clipboard;
using MultiClip.Network;

namespace MultiClip.ViewModels
{
    public class ColorViewModel : ClipboardViewModel
    {
        public ColorViewModel(ClipboardState state, DateTime dateTime, 
            Action<ClipboardViewModel> onPaste, Action<ClipboardViewModel> onDelete) 
                : base(state, dateTime, onPaste, onDelete)
        {
        }

        public Brush BackgroundColor { get; set; }
        public Brush TextColor { get; set; }
        public string Text { get; set; }
    }
}
