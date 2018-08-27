using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace MultiClip.Utilities
{
    public static class Exceptions
    {
        private const bool IsDebugBuild =
        #if DEBUG
            true;
        #else
            false;
        #endif

        private const string MessageBoxTitle = 
            "MultiClip Error Report";

        private const string NotifyMessage =
            "MultiClip ran into a problem. " +
            "We've collected some error info and we will be looking into this issue. ";

        private const string NotifyCriticalMessage =
            "MultiClip ran into a problem and needs to restart. " +
            "We've collected some error info and we will be looking into this issue. ";

        /// <summary>
        /// Displays a message box, containing information about the exception
        /// and shows it to the user.
        /// </summary>
        public static void Notify(this Exception e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var message = IsDebugBuild ? e.ToString() : NotifyMessage;
                MessageBox.Show(message, MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        /// <summary>
        /// Displays a message box, containing information about the exception,
        /// shows it to the user and closes the application.
        /// </summary>
        public static void NotifyCritical(this Exception e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var message = (IsDebugBuild ? e.ToString() : NotifyCriticalMessage) + "\nDo you want MultiClip to start again automatically?";
                var result = MessageBox.Show(message, MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var modulePath = Process.GetCurrentProcess().MainModule.FileName;
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c timeout /t 3 & \"{modulePath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });
                }
                Application.Current.Shutdown(1);
            });
        }
    }
}
