using System;
using MultiClip.Utilities;

namespace MultiClip.ViewModels
{
    public class RemoteHostViewModel : ObservableObject
    {
        private bool _isEnabled;

        public Guid MachineGuid { get; set; }
        public string DisplayName { get; set; }
        public bool IsEnabled { get => _isEnabled; set { _isEnabled = value; NotifyPropertyChanged(); } }
    }
}
