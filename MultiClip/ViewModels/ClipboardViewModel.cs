using System;
using System.Windows.Input;
using MultiClip.Clipboard;
using MultiClip.Network;
using MultiClip.Utilities;

namespace MultiClip.ViewModels
{
    public abstract class ClipboardViewModel : ObservableObject
    {
        private ClipboardState _state;
        private bool _isCurrent;
        private DateTime _dateTime;
        Action<ClipboardViewModel> _onPaste;
        Action<ClipboardViewModel> _onDelete;

        public ClipboardViewModel(ClipboardState state, DateTime dateTime,
            Action<ClipboardViewModel> onPaste, Action<ClipboardViewModel> onDelete)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _dateTime = dateTime;
            _isCurrent = false;
            _onPaste = onPaste ?? throw new ArgumentNullException(nameof(onPaste));
            _onDelete = onDelete ?? throw new ArgumentNullException(nameof(onDelete));
        }

        public ClipboardState State => _state;
        public bool IsCurrent { get => _isCurrent; set { _isCurrent = value; NotifyPropertyChanged(); } }
        public DateTime DateTime { get => _dateTime; set { _dateTime = value; NotifyPropertyChanged(); } }
        public bool CanDelete => _onDelete != null;

        public string DateTimeString => Localization.ToLocalTimepointString(DateTime);

        public ICommand PasteCommand => new DelegateCommand(() => _onPaste?.Invoke(this));
        public ICommand DeleteCommand => new DelegateCommand(() => _onDelete?.Invoke(this));

    }
}
