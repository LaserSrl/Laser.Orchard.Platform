using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.Drivers {
    public class UserProviderDisplayPartDriver : ContentPartDriver<UserProviderDisplayPart> {
        private const string TemplateName = "UserProviderDisplay";
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserProviderServices _userProviderServices;

        public UserProviderDisplayPartDriver(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IUserProviderServices userProviderServices) {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _userProviderServices = userProviderServices;
        }

        protected override string Prefix {
            get {
                return "UserProviderDisplay";
            }
        }

        protected override DriverResult Editor(UserProviderDisplayPart part, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(StandardPermissions.SiteOwner, _authenticationService.GetAuthenticatedUser(), part))
                return null;

            return ContentShape("UserProviderDisplay", () => {
                var model = new UserProviderDisplayViewModel();
                if (part.ContentItem.ContentType == "User") {
                    var providers = _userProviderServices.Get(part.ContentItem.Id).Select(x=>x.ProviderName);
                    if (providers != null && providers.Count()>0) {
                        model.Providers = providers;
                    } else {
                        model.Providers = new List<string> { "Local" };
                    }
                } else {
                    model.Providers = new List<string> { "N/A" };
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
            });
        }
    }
}