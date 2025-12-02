using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ERPAdmin
{
    public class CryptoHelper
    {

        public static string RsaEncrypt(string plainText, string publicKeyPath)
        {
            var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(publicKeyPath);
            var rsa = cert.GetRSAPublicKey();

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encryptedBytes);
        }

        // AES Encrypt (used to encrypt request data with 'sek')
        public static string AesEncrypt(string plainText, string keyBase64)
        {
            byte[] key = Convert.FromBase64String(keyBase64);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.GenerateIV(); // Random IV
            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Prepend IV to cipher
            byte[] combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(combined);
        }

        // AES Decrypt (used to decrypt 'sek' and responses)
        public static string AesDecrypt(string cipherBase64, string keyBase64)
        {
            byte[] key = Convert.FromBase64String(keyBase64);
            byte[] cipherData = Convert.FromBase64String(cipherBase64);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV
            byte[] iv = new byte[16];
            byte[] cipherText = new byte[cipherData.Length - 16];
            Buffer.BlockCopy(cipherData, 0, iv, 0, 16);
            Buffer.BlockCopy(cipherData, 16, cipherText, 0, cipherText.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
