using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MarcoERP.WpfUI.Services
{
    /// <summary>
    /// تخزين بيانات تسجيل الدخول محلياً بتشفير AES مرتبط بالجهاز والمستخدم.
    /// يستخدم لميزة "تذكرني" في شاشة تسجيل الدخول.
    /// </summary>
    internal sealed class SavedCredential
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    internal static class CredentialStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MarcoERP", "credentials.dat");

        /// <summary>حفظ بيانات الدخول مشفّرة على القرص.</summary>
        public static void Save(string username, string password)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(new SavedCredential
                {
                    Username = username,
                    Password = password
                });

                var encrypted = Encrypt(json);
                File.WriteAllBytes(FilePath, encrypted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CredentialStore.Save error: {ex.Message}");
            }
        }

        /// <summary>قراءة بيانات الدخول المحفوظة. يعيد null إن لم توجد.</summary>
        public static SavedCredential Load()
        {
            if (!File.Exists(FilePath))
                return null;

            try
            {
                var encrypted = File.ReadAllBytes(FilePath);
                var json = Decrypt(encrypted);
                return JsonSerializer.Deserialize<SavedCredential>(json);
            }
            catch
            {
                Clear();
                return null;
            }
        }

        /// <summary>حذف بيانات الدخول المحفوظة.</summary>
        public static void Clear()
        {
            try
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
            }
            catch { /* ignore */ }
        }

        #region AES Encryption

        private static byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKey();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // IV (16 bytes) + CipherText
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            return result;
        }

        private static string Decrypt(byte[] data)
        {
            if (data == null || data.Length < 17)
                throw new InvalidOperationException("Invalid credential data.");

            using var aes = Aes.Create();
            aes.Key = DeriveKey();

            var iv = new byte[16];
            Buffer.BlockCopy(data, 0, iv, 0, 16);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(data, 16, data.Length - 16);
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>مفتاح مشتق من اسم الجهاز واسم المستخدم — لا يعمل إلا على نفس الحساب.</summary>
        private static byte[] DeriveKey()
        {
            var seed = Environment.MachineName + "|MarcoERP_2024_CredStore|" + Environment.UserName;
            return SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        }

        #endregion
    }
}
