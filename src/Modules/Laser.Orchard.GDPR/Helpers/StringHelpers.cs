using System;
using System.Security.Cryptography;

namespace Laser.Orchard.GDPR.Helpers {
    public static class StringHelpers {
        
        /// <summary>
        /// Generates a unique string based off the one this method is applied on. The
        /// method uses the original string as a starting point for processe that should
        /// lead to a unique string in a non-reversible way.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string GenerateUniqueString(this string original) {
            return (original ?? "" + DateTime.UtcNow.ToString())
                .HMACSHA512(Guid.NewGuid().ToString());
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
        }
    }
}