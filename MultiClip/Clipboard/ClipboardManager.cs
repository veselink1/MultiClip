using System;
using System.Threading.Tasks;
using System.Windows;
using MultiClip.Utilities;

namespace MultiClip.Clipboard
{
    /// <summary>
    /// Allows the user to track, restore and persist previous 
    /// clipboard states.
    /// </summary>
    public sealed class ClipboardManager
    {
        // The retry count for all clipboard-related operations.
        private const int RetryCount = 10;
        // The retry delay for all clipboard-related operations.
        private const int RetryDelay = 100;

        // A reference to the owner window.
        private readonly Window _owner;

        // Is a clipboard switch currently in progress.
        // Used so that the ClipboardUpdated event is not triggered
        // when the clipboard state is being set.
        private bool _ignoreNextUpdate;

        // A clipboard format listener.
        // Used for tracking changes to the system clipboard.
        private readonly ClipboardFormatListener _formatListener;

        // A reference to the logger instance.
        private readonly ILogger _logger = Logger.Default;

        // A weak reference to the current clipboard state.
        // We are using a weak reference to allow for the state to be
        // garbage collected, in case it is not stored by the user 
        // of the class or discarded deliberately.
        private WeakReference<ClipboardState> _currentStateRef;

        // An integer counting the number of times the clipboard
        // has been updated. Used for short-circuiting concurrent read operations.
        private int _lastUpdateId;

        /// <summary>
        /// Notifies that the clipboard state has been changed externally.
        /// </summary>
        public event Action<ClipboardState> StateChanged;

        public ClipboardManager(Window owner)
        {
            _ignoreNextUpdate = false;
            _owner = owner;
            _formatListener = new ClipboardFormatListener(_owner);
            _formatListener.ClipboardUpdated += FormatListener_ClipboardUpdated;
            _currentStateRef = new WeakReference<ClipboardState>(null);
            _lastUpdateId = 0;
        }
        
        public async Task<ClipboardState> GetStateAsync()
        {
            // Try to get the state from the reference.
            if (!_currentStateRef.TryGetTarget(out ClipboardState state))
            {
                // If the target cannot be obtained,
                // read the state from the clipboard.
                state = await _owner.Dispatcher.RetryOnErrorAsync(
                    () => ClipboardHelper.GetState(),
                    RetryCount,
                    RetryDelay);
            }

            // Update the state reference.
            UpdateStateRef(state);
            return state;
        }

        /// <summary>
        /// Sets the clipboard state asynchronously.
        /// </summary>
        /// <param name="state">The new clipboard state.</param>
        /// <returns></returns>
        public async Task SetStateAsync(ClipboardState state)
        {
            // Get the current state, in order to support 
            // reverting to it in case of failure.
            ClipboardState prevState = await GetStateAsync();

            _ignoreNextUpdate = true;
            try
            {
                await _owner.Dispatcher.RetryOnErrorAsync(
                    () => ClipboardHelper.SetState(state),
                    RetryCount,
                    RetryDelay);
            }
            catch (Exception e)
            {
                await _owner.Dispatcher.RetryOnErrorAsync(
                    () => ClipboardHelper.SetState(prevState),
                    RetryCount,
                    RetryDelay);
                throw e;
            }

            // Update the state reference.
            UpdateStateRef(state);
        }

        public void IgnoreNextUpdate()
        {
            _ignoreNextUpdate = true;
        }
        
        /// <summary>
        /// Updates the weak reference to the current clipboard state.
        /// </summary>
        private void UpdateStateRef(ClipboardState state)
        {
            _currentStateRef = new WeakReference<ClipboardState>(state, trackResurrection: false);
        }

        /// <summary>
        /// Called when the clipboard state has been changed.
        /// </summary>
        private async void FormatListener_ClipboardUpdated()
        {
            if (_ignoreNextUpdate)
            {
                _ignoreNextUpdate = false;
                return;
            }

            ClipboardState newState = null;
            int updateId = ++_lastUpdateId;
            
            try
            {
                try
                {
                    newState = await _owner.Dispatcher.RetryOnErrorAsync(
                        () => ClipboardHelper.GetState(DataConversions.FilterInexpendable),
                        RetryCount,
                        RetryDelay);
                }
                catch
                {
                    await Task.Delay(1000);
                    newState = ClipboardHelper.GetState(DataConversions.FilterInexpendable);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarn(LogEvents.ClipbdReadErr, e);
                e.Notify();
                return;
            }

            if (_currentStateRef.TryGetTarget(out ClipboardState oldState)
                && ClipboardParser.WeakEquals(oldState, newState))
            {
                return;
            }

            if (updateId != _lastUpdateId)
            {
                return;
            }

            // Update the state reference.
            UpdateStateRef(newState);

            // Notify that the state has been changed.
            StateChanged?.Invoke(newState);
        }
    }
}
