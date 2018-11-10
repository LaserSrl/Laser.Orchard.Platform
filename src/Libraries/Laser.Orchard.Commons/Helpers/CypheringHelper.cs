using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.Commons.Helpers {
    public static class CypheringHelper {
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private const string initVector = "lerSavct9vHtl88r";
        // This constant is used to determine the keysize of the encryption algorithm
        private const int keysize = 256;
        //Encrypt
        public static string EncryptString(this string plainText, string passPhrase) {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
        //Decrypt
        public static string DecryptString(this string cipherText, string passPhrase) {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
        /// <summary>
        /// returns base64 string of message hashed with sha512 algorithm
        /// </summary>
        /// <param name="message">the message to hash</param>
        /// <param name="key">the key to use</param>
        /// <returns></returns>
        public static string HMACSHA512(this string message, string key) {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(key);

            HMACSHA512 hmacsha256 = new HMACSHA512(keyByte);
            byte[] messageBytes = encoding.GetBytes(message);

            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
            //         return (ByteToString(hashmessage));
        }

        //        public static string AesCypher(this string message, string key){
        //        string original = "Here is some data to encrypt!";

        //                // Create a new instance of the Aes
        //                // class.  This generates a new key and initialization 
        //                // vector (IV).
        //                using (Aes myAes = Aes.Create())
        //                {

        //                    // Encrypt the string to an array of bytes.
        //                    byte[] encrypted = EncryptStringToBytes_Aes(original, 
        //myAes.Key, myAes.IV);

        //                    // Decrypt the bytes to a string.
        //                    string roundtrip = DecryptStringFromBytes_Aes(encrypted, 
        //myAes.Key, myAes.IV);

        //                    //Display the original data and the decrypted data.
        //                    Console.WriteLine("Original:   {0}", original);
        //                    Console.WriteLine("Round Trip: {0}", roundtrip);
        //                }}

    }
}
