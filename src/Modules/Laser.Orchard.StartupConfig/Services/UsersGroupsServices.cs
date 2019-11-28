using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IUsersGroupsServices : IDependency {
        bool SameGroup(Int32 ownerId, Int32 currentUserId);

    }
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsServices : IUsersGroupsServices {
        private readonly IContentManager _contentManager;
        public UsersGroupsServices(IContentManager contentManager) {
            _contentManager = contentManager;
        }
        public bool SameGroup(Int32 ownerId, Int32 currentUserId) {
            string ownerGroups = _contentManager.Query<UserPart, UserPartRecord>()
                     .Where(x => x.Id == ownerId).List().FirstOrDefault().ContentItem.As<UsersGroupsPart>().UserGroup;
            string currentUserGroups = _contentManager.Query<UserPart, UserPartRecord>()
                .Where(x => x.Id == currentUserId).List().FirstOrDefault().ContentItem.As<UsersGroupsPart>().UserGroup;
            if (!String.IsNullOrWhiteSpace(ownerGroups) && !String.IsNullOrWhiteSpace(currentUserGroups)) {
                List<int> primo = ownerGroups.Split(',').Select(int.Parse).ToList();
                List<int> secondo = currentUserGroups.Split(',').Select(int.Parse).ToList();
                var intersezione = primo.Intersect(secondo);
                if (intersezione.Count() > 0)
                    return true;
            } else if (String.IsNullOrWhiteSpace(ownerGroups) && String.IsNullOrWhiteSpace(currentUserGroups)) {
                return true;
            } else {
                return false;
            }
            return false;
        }

    }

    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class ValidateUserGroup : ValidationAttribute {
        public ValidateUserGroup() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var model = (UsersGroupsVM)value;
            if (model.Required) { // Required
                if (model.GroupNumber==null || model.GroupNumber.Count()<=0){
                    return new ValidationResult("You have to select at least one group");
                }
            }
            return ValidationResult.Success;
        }


    }

}