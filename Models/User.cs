using System.Text;
using Microsoft.AspNetCore.Identity; 
using System.Security.Cryptography; 

namespace ConstructionApp.Models
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; } = default!;
        public string Address { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int? WorkSiteId { get; set; }
        public WorkSite? WorkSite { get; set; }
        public string EncryptedPassword { get; set; } = string.Empty;
        public string? Avatar {  get; set; }

        public ICollection<AttachFile> AttachFiles { get; set; } = new List<AttachFile>();
    }
    public static class EncryptionHelper
    {
        // NOTE: Use a secure, random key in production, and store it securely (e.g., environment variable, Azure Key Vault)
        private static readonly string Key = "A1B2C3D4E5F6G7H8"; // Must be 16 characters for AES-128

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            var iv = aes.IV;
            var encrypted = ms.ToArray();

            var result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string encryptedText)
        {
            var fullCipher = Convert.FromBase64String(encryptedText);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }

}
