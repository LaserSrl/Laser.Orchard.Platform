using Laser.Orchard.HiddenFields.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields {
    public class HiddenFieldsPermissions : IPermissionProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IHiddenFieldService _hiddenFieldService;

        public HiddenFieldsPermissions(
            IContentDefinitionManager contentDefinitionManager,
            IHiddenFieldService hiddenFieldService) {

            _contentDefinitionManager = contentDefinitionManager;
            _hiddenFieldService = hiddenFieldService;

        }

        //With this permission, we give admins the ability to modify the hidden fields' values in the back-end
        public static readonly Permission MayEditHiddenFields = new Permission {
            Description = "A user with this permission is able to edit the values in hidden fields. Be careful as that may have unwanted side effects.",
            Name = "MayEditHiddenFields"
        };
        //With this permission, we give admins the ability to modify the hidden fields' values in the back-end
        public static readonly Permission MayEditOwnHiddenFields = new Permission {
            Description = "Edit own hidden string fields",
            Name = "MayEditOwnHiddenFields",
            ImpliedBy = new[] { MayEditHiddenFields }
        };
        //With this permission, we give users the ability to see the values in the hidden fields in the back-end
        public static readonly Permission MaySeeHiddenFields = new Permission {
            Description = "A user with this permission is able to see the values of the hidden fields.",
            Name = "MaySeeHiddenFields",
            ImpliedBy = new[] { MayEditHiddenFields }
        };
        //With this permission, we give admins the ability to modify the hidden fields' values in the back-end
        public static readonly Permission MaySeeOwnHiddenFields = new Permission {
            Description = "See own hidden string fields",
            Name = "MaySeeOwnHiddenFields",
            ImpliedBy = new[] { MaySeeHiddenFields, MayEditOwnHiddenFields }
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            var permissions = new List<Permission>();

            permissions.Add(MayEditHiddenFields);
            permissions.Add(MayEditOwnHiddenFields);
            permissions.Add(MaySeeHiddenFields);
            permissions.Add(MaySeeOwnHiddenFields);

            IEnumerable<ContentPartDefinition> partDefinitions = _contentDefinitionManager.ListPartDefinitions();

            // LINQ.
            var tmp = partDefinitions
                .Where(cpd =>
                    cpd.Fields.Any(cpfd =>
                        cpfd.FieldDefinition.Name.Equals("HiddenStringField")))
                .SelectMany(cpd => cpd.Fields
                    .Where(cpfd =>
                        cpfd.FieldDefinition.Name.Equals("HiddenStringField"))
                        .Select(cpfd => new[] {
                            _hiddenFieldService.GetAllHiddenPermission(cpd.Name, cpfd.Name),
                            _hiddenFieldService.GetSeeAllHiddenPermission(cpd.Name, cpfd.Name),
                            _hiddenFieldService.GetOwnHiddenPermission(cpd.Name, cpfd.Name),
                            _hiddenFieldService.GetSeeOwnHiddenPermission(cpd.Name, cpfd.Name)
                            }));
            permissions.AddRange(tmp.SelectMany(p => p));

            return permissions;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype{
                    Name = "Administrator",
                    Permissions =  GetPermissions().ToArray()
                },
                new PermissionStereotype{
                    Name = "Authenticated",
                    Permissions =  new Permission[] { MaySeeHiddenFields }
                },
                new PermissionStereotype{
                    Name = "Author",
                    Permissions =  new Permission[] { MaySeeHiddenFields }
                },
                new PermissionStereotype{
                    Name = "Contributor",
                    Permissions =  new Permission[] { MaySeeHiddenFields }
                },
                new PermissionStereotype{
                    Name = "Editor",
                    Permissions =  new Permission[] { MaySeeHiddenFields }
                },
                new PermissionStereotype{
                    Name = "Moderator",
                    Permissions =  new Permission[] { MaySeeHiddenFields }
                },
            };
        }
    }
}