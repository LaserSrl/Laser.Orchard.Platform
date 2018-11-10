using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.Handlers {
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsHandler : ContentHandler {

         private readonly IUsersGroupsSettingsService _groupsServices;

         public UsersGroupsHandler(IRepository<UsersGroupsPartRecord> repository, IUsersGroupsSettingsService groupsServices) {
            Filters.Add(StorageFilter.For(repository));
            OnActivated<UsersGroupsPart>(SetUpCustomPart);
             _groupsServices=groupsServices;
        }
         private void SetUpCustomPart(ActivatedContentContext content, UsersGroupsPart part) {
             part._userGroups.Loader(() => {
                 if (String.IsNullOrWhiteSpace(part.UserGroup)) return null;
                 return _groupsServices.GetGroups(part.UserGroup.Split(',').Select(s => Convert.ToInt32(s)).ToArray());
             });
         }
        //protected override void Loaded(LoadContentContext context) {
        //    base.Loaded(context);

        //    var usersGroupsPart = (UsersGroupsPart)context.ContentItem.Parts.SingleOrDefault(x => x.PartDefinition.Name == "UsersGroupsPart");

        //    if (usersGroupsPart == null) {
        //        return;
        //    }
        //    usersGroupsPart._userGroups.Loader(x => {
        //        if (String.IsNullOrWhiteSpace(usersGroupsPart.UserGroup)) return null;
        //        return _groupsServices.GetGroups(usersGroupsPart.UserGroup.Split(',').Select(s=>Convert.ToInt32(s)).ToArray());
        //    });

        //}

    }
}
