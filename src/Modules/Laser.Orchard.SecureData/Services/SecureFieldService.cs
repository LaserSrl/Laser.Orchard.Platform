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

        public Localizer T { get; set; }

        public SecureFieldService(IEncryptionService encryptionService, ShellSettings shellSettings) {
            _encryptionService = encryptionService;
            _shellSettings = shellSettings;

            T = NullLocalizer.Instance;
        }

        private string DecodeString(string str) {
            return Encoding.UTF8.GetString(Decode(Convert.FromBase64String(str)));
        }

        public string EncodeString(string str, string algorithmIV) {
            //return Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(str)));
            return Convert.ToBase64String(Encode(Encoding.UTF8.GetBytes(str), Encoding.UTF8.GetBytes(algorithmIV)));
        }

        public string HashString(string str, string salt) {
            var saltBytes = salt.ToByteArray();
  
            string hashedString = ComputeHashBase64(DefaultHashAlgorithm, saltBytes, str);
            
            return hashedString;
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

        public void HashValue(HashedStringField field, string value) {
            if (value != null) {
                field.Value = HashString(value, field.Salt);
            } else {
                field.Value = null;
            }
        }

        public bool IsValueEqual(ContentPart part, EncryptedStringField field, string value) {
            return string.Equals(field.Value, EncodeString(value, part.PartDefinition.Name + "." + field.Name), StringComparison.Ordinal);
        }

        public Permission GetOwnPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            // TODO: Permission management for HashedStringFields.
            return new Permission {
                Description = T("Manage own {0} encrypted string fields", fieldFullName).Text,
                Name = "ManageOwnEncryptedStringFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    EncryptedStringFieldPermissions.ManageOwnEncryptedStringFields,
                    GetAllPermission(partName, fieldName)
                }
            };
        }

        public Permission GetOwnPermission(ContentPart part, EncryptedStringField field) {
            return GetOwnPermission(part.PartDefinition.Name, field.Name);
        }

        public Permission GetOwnPermission(ContentPart part, HashedStringField field) {
            return GetOwnPermission(part.PartDefinition.Name, field.Name);
        }

        public Permission GetAllPermission(string partName, string fieldName) {
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
            return GetAllPermission(part.PartDefinition.Name, field.Name);
        }

        public Permission GetAllPermission(ContentPart part, HashedStringField field) {
            return GetAllPermission(part.PartDefinition.Name, field.Name);
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

        #region "Symmetric Encryption Algorithm"
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

    }
}