using Laser.Orchard.SecureData.Fields;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.SecureData.Services {
    public interface ISecureFieldService : IDependency {

        #region Encryption
        string EncodeString(string str, string algorithmIV);
        void EncodeValue(ContentPart part, EncryptedStringField field, string value);
        string DecodeValue(EncryptedStringField field);
        bool IsValueEqual(ContentPart part, EncryptedStringField field, string value);
        // GetPermission for EncryptedStringField.
        Permission GetOwnPermission(ContentPart part, EncryptedStringField field);
        Permission GetAllPermission(ContentPart part, EncryptedStringField field);
        Permission GetOwnEncryptedPermission(string partName, string fieldName);
        Permission GetAllEncryptedPermission(string partName, string fieldName);
        #endregion

        #region Hash
        string HashString(string str, string salt, string hashAlgorithm);
        void HashValue(HashedStringField field, string value);
        bool IsValueEqual(HashedStringField field, string value);
        // GetPermission for HashedStringField.
        Permission GetOwnPermission(ContentPart part, HashedStringField field);
        Permission GetAllPermission(ContentPart part, HashedStringField field);
        Permission GetOwnHashedPermission(string partName, string fieldName);
        Permission GetAllHashedPermission(string partName, string fieldName);
        #endregion

        #region Common

        #endregion
    }
}
