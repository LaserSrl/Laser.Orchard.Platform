using Laser.Orchard.SecureData.Fields;
using Laser.Orchard.SecureData.Security;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;

namespace Laser.Orchard.SecureData.Services {
    public class SecureFieldService : ISecureFieldService {
        private const string PBKDF2 = "PBKDF2";
        private const string DefaultHashAlgorithm = PBKDF2;

        private readonly IEncryptionService _encryptionService;
        private readonly ShellSettings _shellSettings;
        private readonly IAppConfigurationAccessor _appConfigurationAccessor;

        public Localizer T { get; set; }

        public SecureFieldService(IEncryptionService encryptionService, ShellSettings shellSettings, IAppConfigurationAccessor appConfigurationAccessor) {
            _encryptionService = encryptionService;
            _shellSettings = shellSettings;
            _appConfigurationAccessor = appConfigurationAccessor;

            T = NullLocalizer.Instance;
        }

        #region Encryption 
        private string DecodeString(string str) {
            return Encoding.UTF8.GetString(Decode(Convert.FromBase64String(str)));
        }

        public string EncodeString(string str, string algorithmIV) {
            //return Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(str)));
            return Convert.ToBase64String(Encode(Encoding.UTF8.GetBytes(str), Encoding.UTF8.GetBytes(algorithmIV)));
        }

        public string DecodeValue(EncryptedStringField field) {
            if (field == null || field.Value == null) {
                return null;
            }
            return DecodeString(field.Value);
        }

        public void EncodeValue(ContentPart part, EncryptedStringField field, string value) {
            // Encoding.UTF8.GetBytes can't encode null values.
            if (value != null) {
                field.Value = EncodeString(value, part.PartDefinition.Name + "." + field.Name);
            } else {
                field.Value = null;
            }
        }

        public bool IsValueEqual(ContentPart part, EncryptedStringField field, string value) {
            return string.Equals(field.Value, EncodeString(value, part.PartDefinition.Name + "." + field.Name), StringComparison.Ordinal);
        }

        public Permission GetOwnEncryptedPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            // TODO: Permission management for HashedStringFields.
            return new Permission {
                Description = T("Manage own {0} encrypted string fields", fieldFullName).Text,
                Name = "ManageOwnEncryptedStringFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    EncryptedStringFieldPermissions.ManageOwnEncryptedStringFields,
                    GetAllEncryptedPermission(partName, fieldName)
                }
            };
        }

        public Permission GetOwnPermission(ContentPart part, EncryptedStringField field) {
            return GetOwnEncryptedPermission(part.PartDefinition.Name, field.Name);
        }

        public Permission GetAllEncryptedPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            // TODO: Permission management for HashedStringFields.
            return new Permission {
                Description = T("Manage all {0} encrypted string fields", fieldFullName).Text,
                Name = "ManageAllEncryptedStringFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    EncryptedStringFieldPermissions.ManageAllEncryptedStringFields
                }
            };
        }

        public Permission GetAllPermission(ContentPart part, EncryptedStringField field) {
            return GetAllEncryptedPermission(part.PartDefinition.Name, field.Name);
        }

        #region Symmetric Encryption Algorithm
        // The algorithm is modified from the DefaultEncryptionService because we need IV not to be random to make search functions on EncryptedStringFields work.
        public byte[] Encode(byte[] data, byte[] customIV) {
            // cipherText ::= IV || ENC(EncryptionKey, IV, plainText) || HMAC(SigningKey, IV || ENC(EncryptionKey, IV, plainText))

            byte[] encryptedData;
            byte[] iv;

            using (var ms = new MemoryStream()) {
                using (var symmetricAlgorithm = CreateSymmetricAlgorithm()) {
                    // IV has to have a length of symmetricAlgorithm.BlockSize.
                    iv = ResizeIV(symmetricAlgorithm, customIV);
                    symmetricAlgorithm.IV = iv;

                    using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write)) {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    encryptedData = ms.ToArray();
                }
            }

            byte[] signedData;

            // signing IV || encrypted data
            using (var hashAlgorithm = CreateHashAlgorithm()) {
                signedData = hashAlgorithm.ComputeHash(iv.Concat(encryptedData).ToArray());
            }

            return iv.Concat(encryptedData).Concat(signedData).ToArray();
        }

        public byte[] Decode(byte[] encodedData) {
            // extract parts of the encoded data
            using (var symmetricAlgorithm = CreateSymmetricAlgorithm()) {
                using (var hashAlgorithm = CreateHashAlgorithm()) {
                    var iv = new byte[symmetricAlgorithm.BlockSize / 8];
                    var signature = new byte[hashAlgorithm.HashSize / 8];
                    var data = new byte[encodedData.Length - iv.Length - signature.Length];

                    Array.Copy(encodedData, 0, iv, 0, iv.Length);
                    Array.Copy(encodedData, iv.Length, data, 0, data.Length);
                    Array.Copy(encodedData, iv.Length + data.Length, signature, 0, signature.Length);

                    // validate the signature
                    var mac = hashAlgorithm.ComputeHash(iv.Concat(data).ToArray());

                    if (!mac.SequenceEqual(signature)) {
                        // message has been tampered
                        throw new ArgumentException();
                    }

                    symmetricAlgorithm.IV = iv;

                    using (var ms = new MemoryStream()) {
                        using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Write)) {
                            cs.Write(data, 0, data.Length);
                            cs.FlushFinalBlock();
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        private SymmetricAlgorithm CreateSymmetricAlgorithm() {
            var algorithm = SymmetricAlgorithm.Create(_shellSettings.EncryptionAlgorithm);
            algorithm.Key = _shellSettings.EncryptionKey.ToByteArray();
            return algorithm;
        }

        private HMAC CreateHashAlgorithm() {
            var algorithm = HMAC.Create(_shellSettings.HashAlgorithm);
            algorithm.Key = _shellSettings.HashKey.ToByteArray();
            return algorithm;
        }

        private byte[] ResizeIV(SymmetricAlgorithm algorithm, byte[] iv) {
            var resizedIV = iv.ToList();

            while (resizedIV.Count < algorithm.BlockSize / 8) {
                resizedIV.AddRange(resizedIV);
            }

            return resizedIV.Take(algorithm.BlockSize / 8).ToArray();
        }
        #endregion
        #endregion

        #region Hash
        public string HashString(string str, string salt, string hashAlgorithm) {
            var saltBytes = Convert.FromBase64String(salt);

            string hashedString = ComputeHashBase64(hashAlgorithm, saltBytes, str);

            return hashedString;
        }

        public void HashValue(HashedStringField field, string value) {
            if (value != null) {
                var saltBytes = new byte[0x10];
                using (var random = new RNGCryptoServiceProvider()) {
                    random.GetBytes(saltBytes);
                }
                field.Salt = Convert.ToBase64String(saltBytes);
                field.HashAlgorithm = DefaultHashAlgorithm;
                field.Value = HashString(value, field.Salt, field.HashAlgorithm);
            } else {
                field.Value = null;
            }
        }

        public bool IsValueEqual(HashedStringField field, string value) {
            bool isValid;
            var saltBytes = Convert.FromBase64String(field.Salt);

            if (field.HashAlgorithm == PBKDF2) {
                // We can't reuse ComputeHashBase64 as the internally generated salt repeated calls to Crypto.HashPassword() return different results.
                isValid = Crypto.VerifyHashedPassword(field.Value, Encoding.Unicode.GetString(CombineSaltAndPassword(saltBytes, value)));
            } else {
                isValid = SecureStringEquality(field.Value, ComputeHashBase64(field.HashAlgorithm, saltBytes, value));
            }

            // Migrating older hashes to Default algorithm if necessary and enabled.
            if (isValid && field.HashAlgorithm != DefaultHashAlgorithm) {
                var keepOldConfiguration = _appConfigurationAccessor.GetConfiguration("Orchard.Users.KeepOldPasswordHash");
                if (String.IsNullOrEmpty(keepOldConfiguration) || keepOldConfiguration.Equals("false", StringComparison.OrdinalIgnoreCase)) {
                    field.HashAlgorithm = DefaultHashAlgorithm;
                    field.Value = ComputeHashBase64(field.HashAlgorithm, saltBytes, value);
                }
            }

            return isValid;
        }

        /// <summary>
        /// Compares two strings without giving hint about the time it takes to do so.
        /// </summary>
        /// <param name="a">The first string to compare.</param>
        /// <param name="b">The second string to compare.</param>
        /// <returns><c>true</c> if both strings are equal, <c>false</c>.</returns>
        private bool SecureStringEquality(string a, string b) {
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

        public Permission GetOwnHashedPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            return new Permission {
                Description = T("Manage own {0} hashed string fields", fieldFullName).Text,
                Name = "ManageOwnHashedStringFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    HashedStringFieldPermissions.ManageOwnHashedStringFields,
                    GetAllHashedPermission(partName, fieldName)
                }
            };
        }

        public Permission GetOwnPermission(ContentPart part, HashedStringField field) {
            return GetOwnHashedPermission(part.PartDefinition.Name, field.Name);
        }

        public Permission GetAllHashedPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            return new Permission {
                Description = T("Manage all {0} hashed string fields", fieldFullName).Text,
                Name = "ManageAllHashedStringFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    HashedStringFieldPermissions.ManageAllHashedStringFields
                }
            };
        }

        public Permission GetAllPermission(ContentPart part, HashedStringField field) {
            return GetAllHashedPermission(part.PartDefinition.Name, field.Name);
        }

        private static string ComputeHashBase64(string hashAlgorithmName, byte[] saltBytes, string password) {
            var combinedBytes = CombineSaltAndPassword(saltBytes, password);

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

        private static byte[] CombineSaltAndPassword(byte[] saltBytes, string password) {
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            return saltBytes.Concat(passwordBytes).ToArray();
        }
        #endregion
    }
}