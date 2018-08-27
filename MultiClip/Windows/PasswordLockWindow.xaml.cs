using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MultiClip.Utilities;
using UComponents.Themes;

namespace MultiClip.Windows
{
    /// <summary>
    /// Interaction logic for PasswordLockWindow.xaml
    /// </summary>
    public partial class PasswordLockWindow : Window
    {
        public PasswordLockWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowIconHelper.HideIcon(this);
            ThemeManager.Reapply();
        }

        /// <summary>
        /// Shows the password window and after the user clicks the OK button, 
        /// returns the user input or null, if the user pressed the Cancel button.
        /// <example>
        /// Example: 
        /// <code>
        /// string input = await PasswordLockWindow.ShowAsync();
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="description">The description of the content of the window.</param>
        /// <param name="label">The label above the input box.</param>
        /// <returns>The value the user entered or null.</returns>
        public static Task<SecureString> ShowAsync(Window ownerWindow = null, string title = null, string description = null, string label = null)
        {
            var taskCompletionSource = new TaskCompletionSource<SecureString>();

            bool wasOwnerEnabled = ownerWindow.IsEnabled;
            ownerWindow.IsEnabled = false;
            var window = new PasswordLockWindow
            {
                Owner = ownerWindow,
                Topmost = true,
            };

            if (title != null)
                window.Title = title;
            if (description != null)
                window.DescriptionBlock.Text = description;
            if (label != null) 
                window.PasswordBoxLabel.Text = label;

            window.Show();

            window.OkButton.Click += delegate
            {
                taskCompletionSource.TrySetResult(window.PasswordBox.SecurePassword);
                window.Close();
            };

            window.CancelButton.Click += delegate
            {
                taskCompletionSource.TrySetResult(null);
                window.Close();
            };

            window.PasswordBox.KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    taskCompletionSource.TrySetResult(window.PasswordBox.SecurePassword);
                    window.Close();
                }
            };

            window.Closed += delegate
            {
                taskCompletionSource.TrySetResult(null);
                ownerWindow.IsEnabled = wasOwnerEnabled;
            };

            return taskCompletionSource.Task;
        }
    }
}
