using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Input;
using IWshRuntimeLibrary;
using MultiClip.Intl;
using MultiClip.Models;
using MultiClip.Utilities;
using MultiClip.Windows;
using UComponents.Themes;

namespace MultiClip.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly Window _window;
        private readonly UserSettings _model;
        private string _secureStringText = "";
        private bool _isRunOnStartupEnabled;
        private ILogger _logger = Logger.Default;

        public SettingsViewModel(Window window, UserSettings userSettings)
        {
            _window = window;
            _model = userSettings;

            _isRunOnStartupEnabled = IsSetToRunAtStartup();
        }

        public bool IsAuthorized { get; private set; } = false;

        public Theme Theme => _model.Theme;

        public string MaxItemsText => MaxItems.ToString();

        public int MaxItems
        {
            get => _model.MaxItems;
            set
            {
                _model.MaxItems = Math.Max(5, Math.Min(value, 100));
                BeginSaveToDisk();
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(MaxItemsText));
            }
        }

        public string PrivateStringText
        {
            get => _secureStringText;
            set { _secureStringText = value; NotifyPropertyChanged(); }
        }

        public bool IsRunAtStartupEnabled
        {
            get => _isRunOnStartupEnabled;
            set { _isRunOnStartupEnabled = value; UpdateStartupBehaviour(); NotifyPropertyChanged(); }
        }

        public bool IsRunAtStartupDisabled
        {
            get => !IsRunAtStartupEnabled;
            set { _isRunOnStartupEnabled = !value; UpdateStartupBehaviour(); NotifyPropertyChanged(); }
        }
        
        public bool IsLightThemeEnabled
        {
            get => _model.Theme == Theme.Light;
            set { if (value) { _model.Theme = Theme.Light; UpdateTheme(); BeginSaveToDisk(); NotifyPropertyChanged(); } }
        }

        public bool IsDarkThemeEnabled
        {
            get => _model.Theme == Theme.Dark;
            set { if (value) { _model.Theme = Theme.Dark; UpdateTheme(); BeginSaveToDisk(); NotifyPropertyChanged(); } }
        }

        public string Locale
        {
            get => _model.Locale;
            set { _model.Locale = value; IntlManager.Apply(new System.Globalization.CultureInfo(value)); BeginSaveToDisk(); NotifyPropertyChanged(); }
        }

        public bool IsBgLocaleEnabled
        {
            get => _model.Locale == "bg-BG";
            set { if (value) { Locale = "bg-BG"; NotifyPropertyChanged(); } }
        }

        public bool IsEnLocaleEnabled
        {
            get => _model.Locale == "en-US";
            set { if (value) { Locale = "en-US"; NotifyPropertyChanged(); } }
        }

        private async void BeginSaveToDisk()
        {
            try
            {
                await _model.SaveToDiskAsync();
            }
            catch (Exception e)
            {
                e.Notify();
                _logger.LogWarn(LogEvents.DiskErr, "Failed to save user settings!", e);
            }
        }

        private bool IsSetToRunAtStartup()
        {
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupPath, "MultiClip.lnk");
            return System.IO.File.Exists(shortcutPath);
        }

        private void UpdateStartupBehaviour()
        {
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupPath, "MultiClip.lnk");

            if (IsRunAtStartupEnabled)
            {
                if (System.IO.File.Exists(shortcutPath))
                {
                    System.IO.File.Delete(shortcutPath);
                }

                Directory.CreateDirectory(startupPath);

                string executablePath = Assembly.GetExecutingAssembly().Location;
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

                shortcut.Description = "Intelligently manage your data in your clipboard and between your devices.";
                shortcut.IconLocation = executablePath;
                shortcut.TargetPath = executablePath;
                shortcut.Save();
            }
            else if (System.IO.File.Exists(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
            }
        }

        private void UpdateTheme()
        {
            ThemeManager.Apply(_model.Theme);
            BeginSaveToDisk();
        }
    }
}
