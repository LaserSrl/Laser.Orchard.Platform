using Laser.Orchard.SecureData.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Security {
    public class EncryptedStringFieldPermissions : IPermissionProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISecureFieldService _secureFieldService;

        public static readonly Permission ManageAllEncryptedStringFields = new Permission {
            Description = "Manage all encrypted string fields",
            Name = "ManageAllEncryptedStringFields"
        };

        public static readonly Permission ManageOwnEncryptedStringFields = new Permission {
            Description = "Manage own encrypted string fields",
            Name = "ManageOwnEncryptedStringFields",
            ImpliedBy = new Permission[] {
                ManageAllEncryptedStringFields
            }
        };

        public EncryptedStringFieldPermissions(IContentDefinitionManager contentDefinitionManager, ISecureFieldService secureFieldService) {
            _contentDefinitionManager = contentDefinitionManager;
            _secureFieldService = secureFieldService;
        }

        public virtual Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = GetPermissions().ToArray()
                },
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            var permissions = new List<Permission>();

            permissions.Add(ManageAllEncryptedStringFields);
            permissions.Add(ManageOwnEncryptedStringFields);

            IEnumerable<ContentPartDefinition> partDefinitions = _contentDefinitionManager.ListPartDefinitions();

            // LINQ.
            var tmp = partDefinitions
                .Where(cpd =>
                    cpd.Fields.Any(cpfd =>
                        cpfd.FieldDefinition.Name.Equals("EncryptedStringField")))
                .SelectMany(cpd => cpd.Fields
                    .Where(cpfd =>
                        cpfd.FieldDefinition.Name.Equals("EncryptedStringField"))
                        .Select(cpfd => new[] {
                            _secureFieldService.GetAllEncryptedPermission(cpd.Name, cpfd.Name),
                            _secureFieldService.GetOwnEncryptedPermission(cpd.Name, cpfd.Name)}));
            permissions.AddRange(tmp.SelectMany(p => p));

            return permissions;
        }
    }
}