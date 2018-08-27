using System.Collections.ObjectModel;
using MultiClip.Clipboard;

namespace MultiClip.Models
{
    /// <summary>
    /// Represents an application state.
    /// </summary>
    public sealed class AppState
    {
        /// <summary>
        /// The current application state.
        /// </summary>
        public static AppState Current { get; set; }

        /// <summary>
        /// The user settings in this application.
        /// </summary>
        public UserSettings UserSettings { get; set; }

        /// <summary>
        /// The list of the local clipboard states.
        /// </summary>
        public ObservableCollection<ClipboardState> LocalStates { get; set; }

        /// <summary>
        /// The list of the known remote hosts and their states.
        /// </summary>
        public ObservableCollection<RemoteClipboardState> RemoteClipboardStates { get; set; }
        
        /// <summary>
        /// Creates a new application state.
        /// </summary>
        public AppState()
        {
        }
    }
}
