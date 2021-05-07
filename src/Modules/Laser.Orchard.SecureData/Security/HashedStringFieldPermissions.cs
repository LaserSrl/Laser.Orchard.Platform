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
    public class HashedStringFieldPermissions : IPermissionProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISecureFieldService _secureFieldService;

        public static readonly Permission ManageAllHashedStringFields = new Permission {
            Description = "Manage all hashed string fields",
            Name = "ManageAllHashedStringFields"
        };

        public static readonly Permission ManageOwnHashedStringFields = new Permission {
            Description = "Manage own hashed string fields",
            Name = "ManageOwnHashedStringFields"
        };

        public HashedStringFieldPermissions(IContentDefinitionManager contentDefinitionManager, ISecureFieldService secureFieldService) {
            _contentDefinitionManager = contentDefinitionManager;
            _secureFieldService = secureFieldService;
        }

        public virtual Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name="Administrator",
                    Permissions = GetPermissions().ToArray()
                },
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            var permissions = new List<Permission>();

            permissions.Add(ManageAllHashedStringFields);
            permissions.Add(ManageOwnHashedStringFields);

            IEnumerable<ContentPartDefinition> partDefinitions = _contentDefinitionManager.ListPartDefinitions();

            // LINQ.
            var tmp = partDefinitions
                .Where(cpd =>
                    cpd.Fields.Any(cpfd =>
                        cpfd.FieldDefinition.Name.Equals("HashedStringField")))
                .SelectMany(cpd => cpd.Fields
                    .Where(cpfd =>
                        cpfd.FieldDefinition.Name.Equals("HashedStringField"))
                        .Select(cpfd => new[] {
                            _secureFieldService.GetAllPermission(cpd.Name, cpfd.Name),
                            _secureFieldService.GetOwnPermission(cpd.Name, cpfd.Name)}));
            permissions.AddRange(tmp.SelectMany(p => p));

            return permissions;
        }
    }
}