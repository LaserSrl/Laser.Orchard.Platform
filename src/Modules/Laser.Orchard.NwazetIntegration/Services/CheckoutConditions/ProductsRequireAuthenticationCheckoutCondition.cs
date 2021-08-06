using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutConditions {
    public class ProductsRequireAuthenticationCheckoutCondition : ICheckoutCondition {
        private readonly IShoppingCart _shoppingCart;
        private readonly INotifier _notifier;
        private readonly IWorkContextAccessor _workContextAccessor;

        public ProductsRequireAuthenticationCheckoutCondition(
            IShoppingCart shoppingCart,
            INotifier notifier,
            IWorkContextAccessor workContextAccessor) {

            _shoppingCart = shoppingCart;
            _notifier = notifier;
            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;

            Url = new UrlHelper(_workContextAccessor.GetContext().HttpContext.Request.RequestContext);
        }

        public Localizer T { get; set; }
        private UrlHelper Url { get; set; }

        public int Priority => 5;

        public bool UserMayCheckout(IUser user, out ActionResult redirect) {
            redirect = new RedirectResult(Url
                .Action("AccessDenied", "Account", new { area = "Orchard.Users" }));

            return UserMayCheckout(user);
        }

        public bool UserMayCheckout(IUser user) {
            var products = _shoppingCart.GetProducts();
            var mayCheckout = products.Any();
            mayCheckout &= !(user == null && products.Any(p => p.Product.AuthenticationRequired));
            if (!mayCheckout) {
                _notifier.Warning(
                    T("Some products in your cart require you to be authenticated. <a href=\"{0}\">Please log on to complete your order.</a>",
                        Url.Action("LogOn", "Account", new { area = "Orchard.Users" })));
            }
            return mayCheckout;
        }
    }
}