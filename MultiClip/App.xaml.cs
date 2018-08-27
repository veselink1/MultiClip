using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using MultiClip.Models;
using MultiClip.Utilities;
using Newtonsoft.Json;

namespace MultiClip
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Mutex s_instanceMutex = new Mutex(initiallyOwned: true, name: "{8F2FCF4E-0DB6-46BF-9E24-F0C80FCAA40E}");
        private static bool s_ownsInstanceMutex = false;

        public App()
        {
            if (s_instanceMutex.WaitOne(TimeSpan.Zero, true))
            {
                s_ownsInstanceMutex = true;
            }
            else
            {
                s_ownsInstanceMutex = false;
                MessageBox.Show("The application is already running.", "MultiClip", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                Environment.Exit(0);
            }

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            // Set up the default logger instance.
            LogLevel logLevel;
            ILogFormatter logFormatter;
            ILogWriter logWriter = new FileLogWriter(Environment.ExpandEnvironmentVariables("%localappdata%\\MultiClip\\Logs.txt"), Encoding.UTF8);
#if DEBUG
            logLevel = LogLevel.Trace;
            logFormatter = new JsonLogFormatter(Formatting.Indented);
#else
            logLevel = LogLevel.Information;
            logFormatter = new JsonLogFormatter(Formatting.None);
#endif
            Logger.Default = new Logger(logLevel, logFormatter, logWriter);

            // Set up the global application state.
            AppState.Current = new AppState
            {
                UserSettings = UserSettings.LoadFromDisk(),
                LocalStates = new ObservableCollection<Clipboard.ClipboardState>(),
                RemoteClipboardStates = new ObservableCollection<RemoteClipboardState>(),
            };

            // Set the required registry keys.
            SetRegistryValues();
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Default.LogCritical(LogEvents.FatalErr, state: e.Exception);
            e.Exception.NotifyCritical();
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Default.LogCritical(LogEvents.FatalErr, state: e.ExceptionObject);
            if (e.ExceptionObject is Exception)
            {
                (e.ExceptionObject as Exception).NotifyCritical();
            }
            else
            {
                new Exception(e.ExceptionObject?.ToString() ?? "null").NotifyCritical();
            }
        }

        private void SetRegistryValues()
        {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            RegistryKey root = Registry.CurrentUser.CreateSubKey(@"Software\\MultiClip");
            root.SetValue("AppPath", appPath);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (s_ownsInstanceMutex)
            {
                s_instanceMutex.ReleaseMutex();
            }
            Logger.Default.WaitWriter();
        }
    }
}
