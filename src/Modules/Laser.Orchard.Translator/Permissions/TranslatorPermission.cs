using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.Translator.Permissions
{
    public class TranslatorPermission : IPermissionProvider
    {
        public virtual Feature Feature { get; set; }

        public static readonly Permission ManageTranslations = new Permission
        {
            Name = "ManageTranslations",
            Description = "Import, export and edit translations"
        };

        public static readonly Permission Translate = new Permission
        {
            Name = "Translate",
            Description = "Translate modules and themes",
            ImpliedBy = new[] { ManageTranslations }
        };

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { Translate, ManageTranslations }
                }
            };
        }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                 Translate,
                 ManageTranslations
            };
        }
    }
}