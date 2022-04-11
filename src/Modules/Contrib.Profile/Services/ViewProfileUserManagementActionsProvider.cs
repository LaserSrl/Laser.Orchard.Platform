using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Contrib.Profile.Services {
    public class ViewProfileUserManagementActionsProvider : IUserManagementActionsProvider {
        private readonly IAuthorizer _authorizer;
        private readonly IFrontEndProfileService _frontEndProfileService;

        public ViewProfileUserManagementActionsProvider(
            IAuthorizer authorizer,
            IFrontEndProfileService frontEndProfileService) {

            _authorizer = authorizer;
            _frontEndProfileService = frontEndProfileService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<Func<HtmlHelper, MvcHtmlString>> UserActionLinks(IUser user) {
            if (!_frontEndProfileService.UserHasNoProfilePart(user)
                && _authorizer.Authorize(Permissions.ViewProfiles, user)) {
                // user has a profilePart and CurrentUser is allowed to view it

                yield return (Func<HtmlHelper, MvcHtmlString>)
                    (Html => Html.ActionLink(
                        T("Profile").ToString(),
                        "Index",
                        new {
                            Area = "Contrib.Profile",
                            Controller = "Home",
                            username = user.UserName
                        },
                        new {
                            target = "_blank"
                        }));
            }
            yield break;
        }
    }
}