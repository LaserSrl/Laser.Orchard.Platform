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
        string EncodeString(string str, string algorithmIV);
        string HashString(string str, string salt);
        void EncodeValue(ContentPart part, EncryptedStringField field, string value);
        void HashValue(HashedStringField field, string value);
        string DecodeValue(EncryptedStringField field);
        bool IsValueEqual(ContentPart part, EncryptedStringField field, string value);
        // GetPermission for EncryptedStringField.
        Permission GetOwnPermission(ContentPart part, EncryptedStringField field);
        Permission GetAllPermission(ContentPart part, EncryptedStringField field);
        // GetPermission for HashedStringField.
        Permission GetOwnPermission(ContentPart part, HashedStringField field);
        Permission GetAllPermission(ContentPart part, HashedStringField field);
        Permission GetOwnPermission(string partName, string fieldName);
        Permission GetAllPermission(string partName, string fieldName);
    }
}
