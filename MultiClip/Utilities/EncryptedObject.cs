using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace MultiClip.Utilities
{
    public class EncryptedObject
    {
        [JsonProperty]
        public byte[] EncryptedData { get; private set; }

        public EncryptedObject()
        {
        }

        private EncryptedObject(byte[] encryptedData)
        {
            EncryptedData = encryptedData;
        }

        public static EncryptedObject Encrypt<T>(T value, byte[] optionalEntropy)
        {
            string json = JsonConvert.SerializeObject(value);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] encryptedData = ProtectedData.Protect(data, optionalEntropy, DataProtectionScope.CurrentUser);
            return new EncryptedObject(encryptedData);
        }

        public T Decrypt<T>(byte[] optionalEntropy)
        {
            byte[] data = ProtectedData.Unprotect(EncryptedData, optionalEntropy, DataProtectionScope.CurrentUser);
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
