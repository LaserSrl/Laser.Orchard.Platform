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
    public class EmptyCartCheckoutCondition : ICheckoutCondition {
        private readonly IShoppingCart _shoppingCart;
        private readonly INotifier _notifier;

        public EmptyCartCheckoutCondition(
            IShoppingCart shoppingCart,
            INotifier notifier) {

            _shoppingCart = shoppingCart;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public int Priority => 15;
        
        public bool UserMayCheckout(IUser user, out ActionResult redirect) {
            // let the service choose where to redirect
            redirect = null;

            return UserMayCheckout(user);
        }

        public bool UserMayCheckout(IUser user) {
            var mayCheckout = _shoppingCart.GetProducts().Any();
            if (!mayCheckout) {
                _notifier.Warning(T("Your Shopping Cart is empty. You have nothing to checkout."));
            }
            return mayCheckout;
        }
        
    }
}