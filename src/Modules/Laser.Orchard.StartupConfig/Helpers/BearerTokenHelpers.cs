using Laser.Orchard.StartupConfig.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Helpers;

namespace Laser.Orchard.StartupConfig.Helpers {
    public static class BearerTokenHelpers {

        #region Serialization of UserData Dictionary
        // Use Newtonsoft.Json to handle this
        public static string SerializeUserDataDictionary(IDictionary<string, string> userDataDictionary) {
            return JsonConvert.SerializeObject(userDataDictionary, Formatting.None);
        }

        public static Dictionary<string, string> DeserializeUserData(string userData) {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(userData);
        }

        #endregion

        #region Cryptographically random strings
        // valid characters for the random string
        private static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        // this can be used as a simpler alternative to System.Web.Security.Membership.GeneratePassword(int, int)
        public static string RandomString(int length = 48) {
            byte[] data = new byte[4 * length];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider()) {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++) {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }
        #endregion

        #region Hashing of secrets
        // This region replicates logic similar to what is done for passwords in Orchard.Users
        public const string PBKDF2 = "PBKDF2";
        public const string DefaultHashAlgorithm = PBKDF2;
        public static void SetSecretHashed(ApiCredentialsPart credentialsPart, string secret) {
            var saltBytes = new byte[0x10];
            using (var random = new RNGCryptoServiceProvider()) {
                random.GetBytes(saltBytes);
            }

            credentialsPart.ApiSecretHash = ComputeHashBase64(credentialsPart.HashAlgorithm, saltBytes, secret);
            credentialsPart.SecretSalt = Convert.ToBase64String(saltBytes);
        }

        public static bool TestSecret(ApiCredentialsPart userApi, string secret) {
            var saltBytes = Convert.FromBase64String(userApi.SecretSalt);

            bool isValid;
            if (userApi.HashAlgorithm == PBKDF2) {
                // We can't reuse ComputeHashBase64 as the internally generated salt repeated 
                // calls to Crypto.HashPassword() return different results.
                isValid = Crypto.VerifyHashedPassword(
                    userApi.ApiSecretHash, 
                    Encoding.Unicode.GetString(
                        CombineSaltAndSecret(saltBytes, secret)));
            } else {
                isValid = SecureStringEquality(
                    userApi.ApiSecretHash, 
                    ComputeHashBase64(userApi.HashAlgorithm, saltBytes, secret));
            }

            return isValid;
        }

        private static bool SecureStringEquality(string a, string b) {
            if (a == null || b == null || (a.Length != b.Length)) {
                return false;
            }

            var aBytes = Encoding.Unicode.GetBytes(a);
            var bBytes = Encoding.Unicode.GetBytes(b);

            var bytesAreEqual = true;
            for (int i = 0; i < a.Length; i++) {
                bytesAreEqual &= (aBytes[i] == bBytes[i]);
            }

            return bytesAreEqual;
        }

        private static string ComputeHashBase64(string hashAlgorithmName, byte[] saltBytes, string secret) {
            var combinedBytes = CombineSaltAndSecret(saltBytes, secret);

            // Extending HashAlgorithm would be too complicated: http://stackoverflow.com/questions/6460711/adding-a-custom-hashalgorithmtype-in-c-sharp-asp-net?lq=1
            if (hashAlgorithmName == PBKDF2) {
                // HashPassword() already returns a base64 string.
                return Crypto.HashPassword(Encoding.Unicode.GetString(combinedBytes));
            } else {
                using (var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName)) {
                    return Convert.ToBase64String(hashAlgorithm.ComputeHash(combinedBytes));
                }
            }
        }

        private static byte[] CombineSaltAndSecret(byte[] saltBytes, string secret) {
            var secretBytes = Encoding.Unicode.GetBytes(secret);
            return saltBytes.Concat(secretBytes).ToArray();
        }

        #endregion
    }
}