using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MultiClip.Clipboard;
using MultiClip.Controls;
using MultiClip.Intl;
using MultiClip.Models;
using MultiClip.Native;
using MultiClip.Network;
using MultiClip.Network.Messages;
using MultiClip.Utilities;
using MultiClip.ViewModels;
using MultiClip.Windows;
using UComponents.Themes;
using Keys = System.Windows.Forms.Keys;

namespace MultiClip
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Describes a type of view that can be presented in the window.
        /// None is meant as the initial value.
        /// </summary>
        private enum ViewVariant
        {
            None,
            LocalClipboard,
            RemoteClipboard,
        }

        public static MainWindow instance;

        /// <summary>
        /// The currently visible ViewVariant.
        /// </summary>
        private ViewVariant _currentViewVariant = ViewVariant.None;

        /// <summary>
        /// An attached boolean property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(MainWindow),
            new PropertyMetadata(default(bool)));

        /// <summary>
        /// Setter for <see cref="IsSelectedProperty"/>.
        /// </summary>
        public static void SetIsSelected(UIElement element, bool value)
        {
            element.SetValue(IsSelectedProperty, value);
        }

        /// <summary>
        /// Getter for <see cref="IsSelectedProperty"/>.
        /// </summary>
        public static bool GetIsSelected(UIElement element)
        {
            return (bool)element.GetValue(IsSelectedProperty);
        }

        /// <summary>
        /// The QuickInfoWindow.
        /// </summary>
        private QuickInfoWindow _quickInfoWindow;

        /// <summary>
        /// The window that provides the shadow of this window.
        /// </summary>
        private WindowDropShadow _dropShadow;

        /// <summary>
        /// Is the window currently visible.
        /// </summary>
        private bool _isVisible;

        /// <summary>
        /// The global hotkey for toggling the window.
        /// </summary>
        private GlobalHotkey _overviewHotkey;

        /// <summary>
        /// The global hotkey for making a secure copy operation
        /// that simply ignores the new state of the clipboard.
        /// </summary>
        private GlobalHotkey _secureCopyHotkey;

        /// <summary>
        /// The UI permission for managing the clipboard.
        /// </summary>
        private UIPermission _clipboardPermission;

        /// <summary>
        /// The clipboard manager.
        /// </summary>
        private ClipboardManager _clipboardManager;

        /// <summary>
        /// The LAN communication server.
        /// </summary>
        private Server _lanServer;

        /// <summary>
        /// The LanScanner instance used for detecting available application instances.
        /// </summary>
        private LanScanner _lanScanner;

        /// <summary>
        /// A reference to the logger instance. 
        /// Should be replaced with a DI instance.
        /// </summary>
        private ILogger _logger = Logger.Default;

        /// <summary>
        /// An instance of a special low-level mouse hook.
        /// Used to hide the window when the user clicks outside of it, because
        /// the standard WPF methods don't work with <see cref="ExtendedWindowStyle.WS_EX_NOACTIVATE"/>.
        /// </summary>
        private LowLevelMouseHook _mouseHook;

        /// <summary>
        /// The chain of currently executing transition tasks.
        /// Used for sequencing of the tasks.
        /// </summary>
        private TaskChain _transitions;

        /// <summary>
        /// The view-model for the current window.
        /// </summary>
        private OverviewViewModel _viewModel;

        /// <summary>
        /// An instance of WindowMessageReceiver used for simple IPC 
        /// communication between the main application and the deskband instance.
        /// </summary>
        private WindowMessageReceiver _wmr;
        /// <summary>
        /// A boolean indicating if the messages received by <see cref="WindowMessageReceiver"/>
        /// should be ignored. True when there is a window transition in progress.
        /// </summary>
        private bool _ignoreIpcMessages = false;

        /// <summary>
        /// Gets a handle to the current window. 
        /// Just for convenience.
        /// </summary>
        private IntPtr Handle => new WindowInteropHelper(this).Handle;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Updates the position of and size of both the 
        /// MainWindow and the shadow Window instances.
        /// </summary>
        private void UpdatePosition()
        {
            Width = SystemParameters.WorkArea.Width * 0.6;
            Top = SystemParameters.WorkArea.Height / 2 - Height / 2;
            Left = SystemParameters.WorkArea.Width / 2 - Width / 2;
        }

        protected override void OnInitialized(EventArgs eventArgs)
        {
            base.OnInitialized(eventArgs);
           
            try
            {
                instance = this;
                // Update the position and size of the window.
                UpdatePosition();
                // Set the window as NO_ACTIVATE, TOOLWINDOW and TOPMOST,
                WindowStyles.SetExtended(this, WindowStyles.GetExtended(this)
                    | ExtendedWindowStyle.WS_EX_NOACTIVATE
                    | ExtendedWindowStyle.WS_EX_TOOLWINDOW
                    | ExtendedWindowStyle.WS_EX_TOPMOST);

                // Apply the user-selected locale.
                IntlManager.Apply(new CultureInfo(AppState.Current.UserSettings.Locale));
                // Apply the user-selected theme.
                ThemeManager.Apply(AppState.Current.UserSettings.Theme);

                // Initialize the drop shadow.
                _dropShadow = new WindowDropShadow(this)
                {
                    Radius = 15.0,
                    Strength = 2.0,
                    Opacity = 0.0,
                };

                // Set the window's view-model.
                _viewModel = new OverviewViewModel();
                // When the remote hosts collection is empty,
                // toggle the appropriate message.
                _viewModel.RemoteHosts.CollectionChanged += delegate
                {
                    if (_viewModel.RemoteHosts.Any())
                    {
                        RemoteHostListEmptyMessage.Visibility = Visibility.Visible;
                    }
                    {
                        RemoteHostListEmptyMessage.Visibility = Visibility.Collapsed;
                    }
                };
                
                InitializeQuickInfoWindow();

                // Request the needed window permissions. (>= Windows 8)
                _clipboardPermission = new UIPermission(PermissionState.Unrestricted)
                {
                    Clipboard = UIPermissionClipboard.AllClipboard
                };

                // Setup the clipboard manager and the according event listeners.
                _clipboardManager = new ClipboardManager(this);
                _clipboardManager.StateChanged += OnClipboardStateChanged;

                // Initially hidden.
                _isVisible = false;
                _transitions = new TaskChain();

                // Initialize the window message receiver with a name.
                _wmr = new WindowMessageReceiver("MULTICLIP_IPC", null);
                // Setup the IPC handler.
                _wmr.MessageReceived += OnMessageReceived;

                // Initialize and set the low-level mouse hook.
                _mouseHook = new LowLevelMouseHook();
                _mouseHook.LButtonDown += OnMouseHookButtonDown;
                _mouseHook.RButtonDown += OnMouseHookButtonDown;
                _mouseHook.XButtonDown += OnMouseHookButtonDown;
                _mouseHook.SetHook();

                // Initialize and register the overview hotkey.
                _overviewHotkey = new GlobalHotkey(HotkeyModfier.Ctrl, Keys.Space);
                _overviewHotkey.Pressed += OnOverviewHotkeyPressed;
                _overviewHotkey.Register();

                // Initialize and register the secure-copy hotkey.
                _secureCopyHotkey = new GlobalHotkey(HotkeyModfier.Ctrl | HotkeyModfier.Alt, Keys.C);
                _secureCopyHotkey.Pressed += OnSecureCopyHotkeyPressed;
                _secureCopyHotkey.Register();

                // Initialize and start the LanScanner.
                _lanScanner = new LanScanner(
                    scanInterval: TimeSpan.FromSeconds(10), 
                    enableBackgroundScanning: true);
                _lanScanner.CurrentHosts.CollectionChanged += OnRemoteHostsChanged;

                // Initialize and start the LAN server.
                _lanServer = new Server();
                _lanServer.Start();

                // Show the help window on the first run of the app.
                if ((bool)Properties.Settings.Default["IsFirstRun"] == true)
                {
                    Properties.Settings.Default["IsFirstRun"] = false;
                    Properties.Settings.Default.Save();
                    HelpWindow helpWindow = new HelpWindow();
                    helpWindow.Show();
                }

                // Bind the syntetic handlers for header clicks.
                LocalHeader.AddClickHandler(delegate
                {
                    SetIsSelected(LocalHeader, true);
                    SetIsSelected(SharedHeader, false);
                    SetCurrentView(ViewVariant.LocalClipboard);
                });

                SharedHeader.AddClickHandler(delegate
                {
                    SetIsSelected(SharedHeader, true);
                    SetIsSelected(LocalHeader, false);
                    SetCurrentView(ViewVariant.RemoteClipboard);
                });

                // Set the view to be initially LocalClipboard.
                SetCurrentView(ViewVariant.LocalClipboard);

                // When a file-drop item is about to open a subwindow,
                // hide the main window.
                FileDropItem.PreviewShellExecute += delegate
                {
                    if (_isVisible)
                    {
                        HideAsync().GetAwaiter();
                    }
                };

                // Update the network states with an interval.
                async Task updateNetworkState()
                {
                    try
                    {
                        await UpdateNetworkStatesAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarn(LogEvents.NetErr, e);
                    }
                    await Task.Delay(5000);
                    Task.Run(updateNetworkState).GetAwaiter();
                }

                Task.Run(updateNetworkState).GetAwaiter();
            }
            catch (Exception e)
            {
                _logger.LogCritical(LogEvents.FatalErr, "Failed to initialize the main window!", e);
                Exceptions.NotifyCritical(e);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Hide();
            _mouseHook?.Dispose();
            _overviewHotkey?.Dispose();
            _dropShadow?.Hide();
            _wmr?.Dispose();
            _quickInfoWindow?.Close();
            _lanServer?.Stop();
            Environment.Exit(0);
        }

        /// <summary>
        /// Handle IPC messages to the <see cref="WindowMessageReceiver"/>.
        /// </summary>
        private void OnMessageReceived(int msg, IntPtr wParam, IntPtr lParam)
        {
            if (_ignoreIpcMessages)
            {
                return;
            }

            // This constant must be used by the application wanting
            // to toggle the MainWindow visibility.
            const int MC_DESKBAND_CLICK = (int)WindowMessage.WM_APP + 1;
            if (msg == MC_DESKBAND_CLICK)
            {
                ToggleVisibilityAsync().GetAwaiter();
            }
        }

        private async void InitializeQuickInfoWindow()
        {
            try
            {
                _quickInfoWindow = new QuickInfoWindow
                {
                    Owner = this
                };
                _quickInfoWindow.Click += OnQuickInfoWindowClicked;
                // Show the start notification content.
                await _quickInfoWindow.ShowContentAsync(
                    GetStartNotificationContent(),
                    TimeSpan.FromSeconds(5)
                 );
            }
            catch (Exception e)
            {
                _logger.LogCritical(LogEvents.UiOpErr, "Failed to initialize the quick information window!", e);
                e.NotifyCritical();
            }
        }
        
        private void OnQuickInfoWindowClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ToggleVisibilityAsync().GetAwaiter();
        }

        private void OnMouseHookButtonDown(object sender, MouseHookEventArgs e)
        {
            // If the window is visible and the mouse pointer is not
            // in the bounds of the window, hide the window.
            if (_isVisible
                && NativeMethods.GetWindowRect(Handle, out var rect)
                && !NativeMethods.PtInRect(ref rect, new NativeMethods.POINT { X = e.Point.X, Y = e.Point.Y }))
            {
                _ignoreIpcMessages = true;
                ToggleVisibilityAsync().ContinueWith(_ => _ignoreIpcMessages = false);
            }
        }

        private void CollectStatesByMemUsage()
        {
            long memUsage = AppState.Current.LocalStates
                .SelectMany(x => x.Items)
                .Select(x => (long)x.SizeInMemory)
                .Sum();

            long maxUsage = (long)(MemStats.GetTotalPhysicalMemory() * (AppState.Current.UserSettings.MaxRamUsage / 100f));

            if (memUsage > maxUsage)
            {
                long diff = memUsage - maxUsage;
                long freeTotal = 0;
                List<ClipboardState> statesToRemove = new List<ClipboardState>();
                List<ClipboardViewModel> viewModelsToRemove = new List<ClipboardViewModel>();
                foreach (var state in AppState.Current.LocalStates.Reverse())
                {
                    if (freeTotal >= diff)
                    {
                        break;
                    }
                    statesToRemove.Add(state);
                    viewModelsToRemove.Add(_viewModel.LocalClipboards.First(x => ReferenceEquals(x.State, state)));
                    freeTotal += state.Items.Sum(x => (long)x.SizeInMemory);
                }
                foreach (var state in statesToRemove)
                {
                    AppState.Current.LocalStates.Remove(state);
                }
                foreach (var viewModel in viewModelsToRemove)
                {
                    _viewModel.LocalClipboards.Remove(viewModel);
                }
            }
        }

        private async void OnClipboardStateChanged(ClipboardState newState)
        {
            try
            {
                CollectStatesByMemUsage();

                var viewModel = CreateClipboardViewModel(newState, optionalHost: null);
                AppState.Current.LocalStates.Insert(0, newState);
                _viewModel.LocalClipboards.Insert(0, viewModel);

                MarkCurrentViewModel(viewModel);

                var content = new ClipboardItemControl { ContentSource = viewModel };
                Task quickInfoTask = _quickInfoWindow.ShowContentAsync(
                    content,
                    TimeSpan.FromSeconds(2));

                if (viewModel is TextViewModel || viewModel is ImageViewModel)
                {
                    var enabledMachineGuids = AppState.Current.UserSettings.EnabledMachineGuids;
                    Task netNotifyTask = Task.WhenAll(AppState.Current.RemoteClipboardStates
                        .Select(x => x.Identity)
                        .Where(x => enabledMachineGuids.Contains(x.MachineGuid))
                        .Select(x => x.SendAsync(new ClipboardNotifyChangedRequest(newState.Id))));

                    try
                    {
                        await Task.WhenAll(
                            netNotifyTask,
                            quickInfoTask);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarn(LogEvents.NetErr, e);
                    }
                }
                else
                {
                    await quickInfoTask;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarn(LogEvents.FatalErr, e);
                e.NotifyCritical();
            }
        }

        private async void OnOverviewHotkeyPressed()
        {
            try
            {
                await ToggleVisibilityAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarn(LogEvents.UiOpErr, e);
                e.Notify();
            }
        }

        private void OnSecureCopyHotkeyPressed()
        {
            _clipboardManager.IgnoreNextUpdate();
            VirtualInput.SendInputCtrlC();
        }

        private async Task ToggleVisibilityAsync()
        {
            UpdatePosition();
            try
            {
                if (_isVisible)
                {
                    await _transitions.AddTask(HideAsync);
                }
                else
                {
                    await _transitions.AddTask(ShowAsync);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarn(LogEvents.UiOpErr, "Visibility transition failed!", e);
                e.Notify();
            }
        }

        private async Task ShowAsync()
        {
            _isVisible = true;
            if (!_mouseHook.IsHookSet)
            {
                _mouseHook.SetHook();
            }
            var animation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            _dropShadow.Show();
            Show();
            NativeMethods.BringWindowToTop(Handle);

            BeginAnimation(OpacityProperty, animation);
            _dropShadow.BeginAnimation(WindowDropShadow.OpacityProperty, animation);
            await Task.Delay((int)animation.Duration.TimeSpan.TotalMilliseconds / 2);
            WindowBlur.Enable(this, preferAcrylic: false);
            await Task.Delay((int)animation.Duration.TimeSpan.TotalMilliseconds / 2);
        }

        private async Task HideAsync()
        {
            _isVisible = false;
            if (_mouseHook.IsHookSet)
            {
                _mouseHook.RemoveHook();
            }
            var animation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            BeginAnimation(OpacityProperty, animation);
            _dropShadow.BeginAnimation(WindowDropShadow.OpacityProperty, animation);

            await Task.Delay((int)animation.Duration.TimeSpan.TotalMilliseconds / 2);
            WindowBlur.Disable(this);
            await Task.Delay((int)animation.Duration.TimeSpan.TotalMilliseconds / 2);
            Hide();
            _dropShadow.Hide();
        }

        private void SetCurrentView(ViewVariant viewVariant)
        {
            if (_currentViewVariant != viewVariant)
            {
                _currentViewVariant = viewVariant;
                if (viewVariant == ViewVariant.LocalClipboard)
                {
                    LocalView.Visibility = Visibility.Visible;
                    LocalList.ItemsSource = _viewModel.LocalClipboards;
                    RemoteView.Visibility = Visibility.Collapsed;
                    RemoteList.ItemsSource = null;
                }
                else if (viewVariant == ViewVariant.RemoteClipboard)
                {
                    LocalView.Visibility = Visibility.Collapsed;
                    LocalList.ItemsSource = null;
                    RemoteView.Visibility = Visibility.Visible;
                    RemoteList.ItemsSource = _viewModel.RemoteClipboards;
                    RemoteHostList.ItemsSource = _viewModel.RemoteHosts;
                }
                else
                {
                    throw new InvalidProgramException("Unknown value for type " + nameof(ViewVariant));
                }
            }
        }

        private async void OnRemoteHostsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    var remoteStates = AppState.Current.RemoteClipboardStates;
                    IEnumerable<Host> newHosts = e.NewItems.Cast<Host>();

                    var enabledMachineGuids = AppState.Current.UserSettings.EnabledMachineGuids;
                    foreach (var host in newHosts)
                    {
                        var remoteState = new RemoteClipboardState
                        {
                            Identity = host,
                            States = new ObservableCollection<ClipboardState>(),
                        };
                        remoteState.States.CollectionChanged += (s, args) => OnHostStatesChanged(s, args, remoteState.Identity);
                        remoteStates.Add(remoteState);
                        var viewModel = new RemoteHostViewModel
                        {
                            MachineGuid = remoteState.Identity.MachineGuid,
                            DisplayName = remoteState.Identity.MachineName,
                            IsEnabled = enabledMachineGuids.Contains(remoteState.Identity.MachineGuid),
                        };
                        viewModel.PropertyChanged += OnRemoteHostPropertyChanged;
                        _viewModel.RemoteHosts.Add(viewModel);
                    }

                    await UpdateNetworkStatesAsync();
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var remoteStates = AppState.Current.RemoteClipboardStates;
                    IEnumerable<Host> removedHosts = e.OldItems.Cast<Host>();

                    var enabledMachineGuids = AppState.Current.UserSettings.EnabledMachineGuids;
                    foreach (var host in removedHosts)
                    {
                        remoteStates.Remove(remoteStates.FirstOrDefault(x => x.Identity.MachineGuid == host.MachineGuid));
                        _viewModel.RemoteHosts.Remove(_viewModel.RemoteHosts.FirstOrDefault(x => x.MachineGuid == host.MachineGuid));
                    }

                    await UpdateNetworkStatesAsync();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarn(LogEvents.NetErr, ex);
            }
        }

        private async Task UpdateNetworkStatesAsync()
        {
            var remoteHosts = AppState.Current.RemoteClipboardStates;
            var enabledMachineGuids = AppState.Current.UserSettings.EnabledMachineGuids;
            var enabledHosts = remoteHosts.Where(x => enabledMachineGuids.Contains(x.Identity.MachineGuid));
            foreach (var remoteHost in enabledHosts)
            {
                // Get the list of currently known state IDs.
                List<Guid> knownStateIds = remoteHost.States.Select(x => x.Id).ToList();
                // Send a request to get any new IDs.
                ClipboardInfoResponse cbInfo = await remoteHost.Identity.SendAsync(new ClipboardInfoRequest(knownStateIds));
                // Get the value of every state that is not known.
                // and add it to the list.
                foreach (var state in cbInfo.States
                    .Where(x => !knownStateIds.Contains(x.Id))
                    .OrderBy(x => x.DateTime))
                {
                    ClipboardStateResponse res = await remoteHost.Identity.SendAsync(new ClipboardStateRequest(state.Id));

                    if (res.IsError)
                    {
                        res.Buffer = null;
                        _logger.LogWarn(LogEvents.NetErr, res);
                        continue;
                    }
                    else
                    {
                        remoteHost.States.Add(new ClipboardState(res.StateGuid, res.DateTime, new ClipboardItem[]
                        {
                            ClipboardItem.FromBuffer(res.Format, res.Buffer, cloneBuffer: false)
                        }));
                    }
                }
            }
        }

        private async void OnRemoteHostPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var viewModel = (RemoteHostViewModel)sender;
            if (e.PropertyName == nameof(RemoteHostViewModel.IsEnabled))
            {
                var enabledMachineIds = AppState.Current.UserSettings.EnabledMachineGuids;
                if (viewModel.IsEnabled)
                {
                    var remoteHost = AppState.Current.RemoteClipboardStates.First(x => x.Identity.MachineGuid == viewModel.MachineGuid);
                    remoteHost.States.Clear();
                    enabledMachineIds.Add(viewModel.MachineGuid);
                    UpdateNetworkStatesAsync().GetAwaiter();
                }
                else
                {
                    foreach (var stateViewModel in _viewModel.RemoteClipboards.Where(x => x.Host.MachineGuid == viewModel.MachineGuid).ToList())
                    {
                        _viewModel.RemoteClipboards.Remove(stateViewModel);
                    }
                    enabledMachineIds.Remove(viewModel.MachineGuid);
                }

                try
                {
                    await AppState.Current.UserSettings.SaveToDiskAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarn(LogEvents.DiskErr, ex);
                }
            }
        }

        private void OnHostStatesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e, Host host)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (var item in e.NewItems.Cast<ClipboardState>())
                    {
                        var viewModel = CreateClipboardViewModel(item, host);
                        _viewModel.RemoteClipboards.Insert(0, viewModel);
                    }
                }
            });
        }

        private UIElement GetStartNotificationContent()
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(16, 24, 16, 16),
            };
            panel.Children.Add(new TextBlock
            {
                Text = "MultiClip (Beta)",
                FontSize = 20,
                FontStyle = FontStyles.Normal,
                FontWeight = FontWeights.Normal,
                Foreground = (Brush)Resources["UColorBaseHigh"],
                Margin = new Thickness(0, 0, 0, bottom: 16),
            });
            panel.Children.Add(new TextBlock
            {
                Text = "Running in the background.\nYou can touch this notification or press \nCtrl+Space to toggle Clipboard Overview.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 15,
                Foreground = (Brush)Resources["UColorBaseMediumHigh"]
            });
            return panel;
        }

        private static readonly Regex MultipleWhitespaceRegex = new Regex("\\s{2,}", RegexOptions.None);

        private ClipboardViewModel CreateClipboardViewModel(ClipboardState state, Host optionalHost)
        {
            ClipboardItem item = ClipboardParser.GetPreferredItem(state.Items, serializable: false);
            if (item == null)
            {
                _logger.LogInfo(LogEvents.UnknClipbdFmt, state.Items.Select(x => new { x.Format, x.FormatName }));
                return new UnknownViewModel(
                    state,
                    DateTime.Now,
                    optionalHost,
                    onPaste: OnPasteStateClicked,
                    onDelete: OnDeleteStateClicked);
            }

            switch (ClipboardParser.GetAbstractFormat(item.Format))
            {
                case AbstractDataFormat.Image:
                    return new ImageViewModel(
                        state,
                        DateTime.Now,
                        optionalHost,
                        onPaste: OnPasteStateClicked,
                        onDelete: OnDeleteStateClicked)
                    {
                        Image = ClipboardParser.ParseImage(item),
                    };
                case AbstractDataFormat.Text:
                    var text = ClipboardParser.ParseText(item);
                    string source = text.Trim();
                    if (source.Length < 50 && ColorParser.TryParse(source, out Color color))
                    {
                        Color textColor = (color.R + color.G + color.B) * (float)color.A / 255 / 3 >= 127
                            ? System.Windows.Media.Colors.Black
                            : System.Windows.Media.Colors.White;
                        return new ColorViewModel(
                            state,
                            DateTime.Now,
                            optionalHost,
                            onPaste: OnPasteStateClicked,
                            onDelete: OnDeleteStateClicked)
                        {
                            BackgroundColor = new SolidColorBrush(color),
                            TextColor = new SolidColorBrush(textColor),
                            Text = text.Trim(),
                        };
                    }
                    else
                    {
                        string textContent = MultipleWhitespaceRegex.Replace(ClipboardParser.ParseText(item), " ");
                        return new TextViewModel(
                            state,
                            DateTime.Now,
                            optionalHost,
                            onPaste: OnPasteStateClicked,
                            onDelete: OnDeleteStateClicked)
                        {
                            Text = textContent,
                        };
                    }
                case AbstractDataFormat.FileDrop:
                    string[] items = ClipboardParser.ParseFileDrop(item)
                        .Where(x => File.Exists(x) || Directory.Exists(x))
                        .ToArray();
                    return new FileDropViewModel(
                        state,
                        DateTime.Now,
                        optionalHost,
                        onPaste: OnPasteStateClicked,
                        onDelete: OnDeleteStateClicked)
                    {
                        Items = items,
                    };
                default:
                    _logger.LogInfo(LogEvents.UnknClipbdFmt, state.Items.Select(x => new { x.Format, x.FormatName }));
                    return new UnknownViewModel(
                        state,
                        DateTime.Now,
                        optionalHost,
                        onPaste: OnPasteStateClicked,
                        onDelete: OnDeleteStateClicked);
            }
        }

        private async void OnPasteStateClicked(ClipboardViewModel viewModel)
        {
            try
            {
                MarkCurrentViewModel(viewModel);
                await _clipboardManager.SetStateAsync(viewModel.State);
                VirtualInput.SendInputCtrlV();
                await ToggleVisibilityAsync();
            }
            catch (Exception e)
            {
                viewModel.IsCurrent = false;
                _logger.LogWarn(LogEvents.PasteErr, "Failed to paste the content to the target window!", e);
                e.Notify();
            }
        }

        private void MarkCurrentViewModel(ClipboardViewModel optViewModel)
        {
            foreach (var otherViewModel in _viewModel.LocalClipboards.Concat(_viewModel.RemoteClipboards))
            {
                otherViewModel.IsCurrent = false;
            }
            if (optViewModel != null)
            {
                optViewModel.IsCurrent = true;
            }
        }

        private void OnDeleteStateClicked(ClipboardViewModel viewModel)
        {
            if (_viewModel.LocalClipboards.Remove(viewModel))
            {
                AppState.Current.LocalStates.Remove(viewModel.State);
            }
            else
            {
                _viewModel.RemoteClipboards.Remove(viewModel);
                foreach (var host in AppState.Current.RemoteClipboardStates)
                {
                    host.States.Remove(viewModel.State);
                }
            }
        }

        private void OnHelpClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                HelpWindow helpWindow = new HelpWindow();
                helpWindow.Show();
                helpWindow.Activate();
            }
            catch (Exception ex)
            {
                _logger.LogWarn(LogEvents.UiOpErr, ex);
                ex.Notify();
            }
        }

        private void OnCloseClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private async void OnSettingsClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (_isVisible)
                {
                    await HideAsync();
                }
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();
                settingsWindow.Activate();
            }
            catch (Exception ex)
            {
                _logger.LogWarn(LogEvents.UiOpErr, "Failed to open SettingsWindow!", ex);
                ex.Notify();
            }
        }

    }
}
