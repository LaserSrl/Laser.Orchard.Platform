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
    public class ProductOutOfStockCheckoutCondition : ICheckoutCondition {
        private readonly IShoppingCart _shoppingCart;
        private readonly INotifier _notifier;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;

        public ProductOutOfStockCheckoutCondition(
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

        public int Priority => 1;

        public bool UserMayCheckout(IUser user, out ActionResult redirect) {
            redirect = new RedirectResult(Url
                .Action("Index", "ShoppingCart", new { area = "Nwazet.Commerce" }));

            return UserMayCheckout(user);
        }

        public bool UserMayCheckout(IUser user) {
            var products = _shoppingCart.GetProducts();
            var notEnoughStock = products.Where(qp =>
                // check stock
                qp.Quantity > qp.Product.Inventory
                // if back orders are allowed, it's like having enough stock
                && !qp.Product.AllowBackOrder
                // if the product is digital, we may have an infinite number of them
                && !(qp.Product.IsDigital && !qp.Product.ConsiderInventory));
            if (notEnoughStock.Any()) {
                _notifier.Warning(
                    T("There isn't enough stock to complete your order for the following products: {0}. <a href=\"{1}\">Please review your cart.</a>",
                        string.Join(", ", notEnoughStock.Select(qp => _contentManager.GetItemMetadata(qp.Product).DisplayText)),
                        Url.Action("Index", "ShoppingCart", new { area = "Nwazet.Commerce" })));

                return false;
            }
            return true;
        }
    }
}