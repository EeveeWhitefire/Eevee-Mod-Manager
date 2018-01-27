using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace EeveexModManager.Classes
{
    public class Cryptographer
    {
        static byte[] additionalEntropy = Encoding.ASCII.GetBytes(Environment.UserName);

        public static string Encrypt(string text)
        {
            byte[] first = Encoding.Unicode.GetBytes(text);

            return Convert.ToBase64String(
                ProtectedData.Protect(first, additionalEntropy, DataProtectionScope.CurrentUser));
        }

        public static string Decrypt(string text)
        {
            byte[] first = Convert.FromBase64String(text);

            return Encoding.Unicode.GetString(
                ProtectedData.Unprotect(first, additionalEntropy, DataProtectionScope.CurrentUser));
        }
    }
}
