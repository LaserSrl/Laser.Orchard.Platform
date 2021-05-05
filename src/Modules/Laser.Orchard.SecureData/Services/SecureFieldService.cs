using Laser.Orchard.SecureData.Fields;
using Laser.Orchard.SecureData.Security;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Laser.Orchard.SecureData.Services {
    public class SecureFieldService : ISecureFieldService {
        private readonly IEncryptionService _encryptionService;
        public Localizer T { get; set; }

        public SecureFieldService(IEncryptionService encryptionService) {
            _encryptionService = encryptionService;

            T = NullLocalizer.Instance;
        }

        private string EncodeString(string str) {
            return Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(str)));
        }

        private string DecodeString(string str) {
            return Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(str)));
        }

        public void EncodeValue(EncryptedStringField field, string value) {
            // Encoding.UTF8.GetBytes can't encode null values.
            if (value != null) {
                field.Value = EncodeString(value);
            } else {
                field.Value = null;
            }
        }

        public string DecodeValue(EncryptedStringField field) {
            if (field == null || field.Value == null) {
                return null;
            }
            return DecodeString(field.Value);
        }

        public bool IsValueEqual(EncryptedStringField field, string value) {
            return string.Equals(field.Value, EncodeString(value), StringComparison.Ordinal);
        }

        public Permission GetOwnPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
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

        public Permission GetAllPermission(string partName, string fieldName) {
            string fieldFullName = partName + "." + fieldName;
            return new Permission {
                Description = T("Manage all {0} encrypted string fields", fieldFullName).Text,
                Name = "ManagAllEncryptedStringFields_" + fieldFullName,
                ImpliedBy = new Permission[] {
                    EncryptedStringFieldPermissions.ManageAllEncryptedStringFields
                }
            };
        }

        public Permission GetAllPermission(ContentPart part, EncryptedStringField field) {
            return GetAllPermission(part.PartDefinition.Name, field.Name);
        }
    }
}