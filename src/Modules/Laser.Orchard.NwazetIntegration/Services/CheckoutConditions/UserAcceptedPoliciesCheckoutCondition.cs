using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutConditions {
    public class UserAcceptedPoliciesCheckoutCondition
        : ICheckoutCondition {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICheckoutPoliciesService _checkoutPoliciesService;
        private readonly INotifier _notifier;
        private readonly dynamic _shapeFactory;

        public UserAcceptedPoliciesCheckoutCondition(
            IWorkContextAccessor workContextAccessor,
            ICheckoutPoliciesService checkoutPoliciesService,
            INotifier notifier,
            IShapeFactory shapeFactory) {

            _workContextAccessor = workContextAccessor;
            _checkoutPoliciesService = checkoutPoliciesService;
            _notifier = notifier;

            T = NullLocalizer.Instance;

            Url = new UrlHelper(_workContextAccessor.GetContext().HttpContext.Request.RequestContext);
        }

        public Localizer T { get; set; }
        private UrlHelper Url { get; set; }
        public int Priority => 5;

        public bool UserMayCheckout(IUser user, out ActionResult redirect) {
            // This method is called during checkout, and is used to actively
            // stop checkout from happening if the condition is not verified.
            redirect = new RedirectResult(Url
                .Action("Index", "ShoppingCart", new { area = "Nwazet.Commerce" }));
            
            var mayCheckout = _checkoutPoliciesService.UserHasAllAcceptedPolicies();
            if (!mayCheckout) {
                _notifier.Warning(T("You need to accept our terms and conditions in order to checkout."));
            }
            return mayCheckout;
        }

        public bool UserMayCheckout(IUser user) {
            // This method is called before checkout, and is used to notify 
            // the user of conditions that must be verified for them to go
            // ahead with checkout. Returning false here would prevent the 
            // "Checkout" button from even displaying. Since the goal is to
            // have the button, but display an "accept the policies" checkbox
            // alongside it, this method should return true regardless of the
            // fact that the user may have accepted the policies or not. The
            // other UserMayCheckout method here is responsible for actually
            // preventing checkout.
            
            return true;
        }

        public IEnumerable<dynamic> AdditionalCheckoutStartShapes() {
            // The shape from this will add the option for the user to accept 
            // checkout policies.
            // There will be one "line" (with a checkbox) for each configured
            // policy that the user hasn't accepted yet.
            // If the user has already accepted a given policy, that line 
            // will not show.

            // The shape will need:
            // - The list of all required policies that are configured for checkout.
            // - The list of all optional policies that are configured for checkout.
            // - The list of all policies that the user has already accepted.

            yield return null;
        }
    }
}