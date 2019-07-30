using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MultiClip.Network;
using MultiClip.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UComponents.Themes;

namespace MultiClip.Models
{
    /// <summary>
    /// Represent's a user's settings.
    /// </summary>
    public class UserSettings
    {
        public class PrivateString
        {
            public string Value { get; set; }
            public DateTime Timestamp { get; set; }
        }

        /// <summary>
        /// The default path for storing the user settings.
        /// </summary>
        public static string DefaultPath => Environment.ExpandEnvironmentVariables("%localappdata%\\MultiClip\\UserSettings.json");

        /// <summary>
        /// Computes the hash used for authorization of the provided value.
        /// </summary>
        private static byte[] ComputeAuthorizationHash(string value) =>
            SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("Authorize$" + value + "$MultiClip"));
        
        /// <summary>
        /// A synchronization object for all IO operations.
        /// </summary>
        private readonly object _ioSyncRoot = new object();

        /// <summary>
        /// The locale chosen by the user.
        /// </summary>
        public string Locale { get; set; } = CultureInfo.CurrentCulture.Name;
        /// <summary>
        /// The theme chosen by the user.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Theme Theme { get; set; } = Theme.Dark;
        /// <summary>
        /// The maximum number of items stored.
        /// </summary>
        public int MaxItems { get; set; } = 10;
        /// <summary>
        /// The unique identifier of this machine.
        /// </summary>
        public Guid MachineGuid { get; set; } = Guid.NewGuid();
        /// <summary>
        /// The list of user-approved machine IDs.
        /// </summary>
        public List<Guid> EnabledMachineGuids { get; set; } = new List<Guid>();

        private static string ToInsecureString(SecureString secureString)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        /// <summary>
        /// Saves the user settings to the disk.
        /// </summary>
        public Task SaveToDiskAsync()
        {
            return Task.Run(() => SaveToDisk());
        }

        /// <summary>
        /// Saves the user settings to the disk.
        /// </summary>
        public void SaveToDisk()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DefaultPath));
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            lock (_ioSyncRoot)
            {
                File.WriteAllText(DefaultPath, json, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Loads the user settings from the disk.
        /// </summary>
        public static Task<UserSettings> LoadFromDiskAsync()
        {
            return Task.Run(() => LoadFromDisk());
        }

        /// <summary>
        /// Loads the user settings from the disk.
        /// </summary>
        public static UserSettings LoadFromDisk()
        {
            if (File.Exists(DefaultPath))
            {
                var json = File.ReadAllText(DefaultPath, Encoding.UTF8);
                try
                {
                    return JsonConvert.DeserializeObject<UserSettings>(json);
                }
                catch (Exception)
                {
                    MessageBoxResult ans = MessageBox.Show("The user settings file was corrupted! Do you want MultiClip to " 
                        + "overwrite the corrupted file with the default configuration?\nPress No to exit.", 
                        "MultiClip", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                    if (ans == MessageBoxResult.Yes)
                    {
                        return CreateDefaultSettings();
                    }
                    else
                    {
                        Environment.Exit(0);
                        return null;
                    }
                }
            }
            else
            {
                return CreateDefaultSettings();
            }
        }

        private static UserSettings CreateDefaultSettings()
        {
            var settings = new UserSettings();
            settings.SaveToDisk();
            return settings;
        }
    }
}
