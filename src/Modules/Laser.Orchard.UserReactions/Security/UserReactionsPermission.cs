using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Security {
    public class UserReactionsPermission : IPermissionProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public virtual Feature Feature { get; set; }

        public UserReactionsPermission(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            Permission[] noPermission = new Permission[0];
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = noPermission
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = noPermission
                },
            };
        }

        public IEnumerable<Permission> GetPermissions() {
            List<Permission> elencoPermission = new List<Permission>();
            var elencoTypes = GetContentTypeUsingReactions();
            foreach (var type in elencoTypes) {
                elencoPermission.Add(new Permission {
                    Name = string.Format("ReactionsFor{0}", type.Name),
                    Description = string.Format("Reactions for {0}", type.Name)
                });
            }
            return elencoPermission;
        }

        private IEnumerable<ContentTypeDefinition> GetContentTypeUsingReactions() {
            var types = _contentDefinitionManager.ListTypeDefinitions().Where(x => x.Parts.Count(y => y.PartDefinition.Name == "UserReactionsPart") > 0).ToList();
            return types;
        }

    }
}