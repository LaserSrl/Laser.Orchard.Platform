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
        string EncodeString(string str, string encryptionKey);
        void EncodeValue(EncryptedStringField field, string value);
        string DecodeValue(EncryptedStringField field);
        bool IsValueEqual(EncryptedStringField field, string value);
        Permission GetOwnPermission(ContentPart part, EncryptedStringField field);
        Permission GetAllPermission(ContentPart part, EncryptedStringField field);
        Permission GetOwnPermission(string partName, string fieldName);
        Permission GetAllPermission(string partName, string fieldName);
    }
}
