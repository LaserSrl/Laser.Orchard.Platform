using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutConditions {
    public class MinimumOrderQuantityCheckoutCondition : ICheckoutCondition {
        private readonly IShoppingCart _shoppingCart;
        private readonly INotifier _notifier;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;

        public MinimumOrderQuantityCheckoutCondition(
            IShoppingCart shoppingCart,
            INotifier notifier,
            IWorkContextAccessor workContextAccessor,
            IContentManager contentManager) {

            _shoppingCart = shoppingCart;
            _notifier = notifier;
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;

            Url = new UrlHelper(_workContextAccessor.GetContext().HttpContext.Request.RequestContext);
        }

        public Localizer T { get; set; }
        private UrlHelper Url { get; set; }

        public int Priority => 2;

        public bool UserMayCheckout(IUser user, out ActionResult redirect) {
            redirect = new RedirectResult(Url
                .Action("Index", "ShoppingCart", new { area = "Nwazet.Commerce" }));

            return UserMayCheckout(user);
        }

        public bool UserMayCheckout(IUser user) {
            var products = _shoppingCart.GetProducts();
            var notEnoughInCart = products.Where(qp =>
                qp.Quantity < qp.Product.MinimumOrderQuantity);
            if (notEnoughInCart.Any()) {
                var productNotes = notEnoughInCart
                    .Select(qp => T("{0} (minimum {1})",
                         _contentManager.GetItemMetadata(qp.Product).DisplayText,
                         qp.Product.MinimumOrderQuantity).Text);
                _notifier.Warning(
                    T("The following products may only be ordered in larger quantities: {0}. <a href=\"{1}\">Please review your cart.</a>",
                        string.Join(", ", productNotes),
                        Url.Action("Index", "ShoppingCart", new { area = "Nwazet.Commerce" })));

                return false;
            }
            return true;
        }
    }
}