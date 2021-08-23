using Laser.Orchard.NwazetIntegration.Services.FacebookShop;
using Nwazet.Commerce.Permissions;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Controllers {
    public class FacebookShopController : Controller {
        private readonly IFacebookShopService _facebookShopService;
        private readonly IAuthorizer _authorizer;

        public FacebookShopController(IFacebookShopService facebookShopService,
            IAuthorizer authorizer) {
            _facebookShopService = facebookShopService;
            _authorizer = authorizer;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult SyncProducts(string redirectUrl) {
            if (!_authorizer.Authorize(CommercePermissions.ManageCommerce,
                null, T("Not authorized to synchronize Facebook Shop catalog"))) {
                return new HttpUnauthorizedResult();
            }

            _facebookShopService.SyncProducts();

            return new RedirectResult(redirectUrl);
        }
    }
}