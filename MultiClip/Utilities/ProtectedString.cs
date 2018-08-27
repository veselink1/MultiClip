using System.Security.Cryptography;
using System.Text;

namespace MultiClip.Utilities
{
    public class ProtectedString
    {
        private byte[] _protectedBytes;

        public ProtectedString(string value, byte[] optionalEntropy)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            _protectedBytes = ProtectedData.Protect(bytes, optionalEntropy, DataProtectionScope.CurrentUser);
        }

        public string Unprotect(byte[] optionalEntropy)
        {
            byte[] bytes = ProtectedData.Unprotect(_protectedBytes, optionalEntropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
