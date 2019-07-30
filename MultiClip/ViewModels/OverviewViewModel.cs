using System.Collections.ObjectModel;
using MultiClip.Utilities;

namespace MultiClip.ViewModels
{
    public class OverviewViewModel : ObservableObject
    {
        private ObservableCollection<ClipboardViewModel> _clipboardItems = new ObservableCollection<ClipboardViewModel>();

        public ObservableCollection<ClipboardViewModel> ClipboardItems
        {
            get => _clipboardItems;
            set { _clipboardItems = value; NotifyPropertyChanged(); }
        }
    }
}
