using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.Policy.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class CheckoutPoliciesOrderAdditionalInformationProvider 
        : BaseOrderAdditionalInformationProvider {

        private readonly ICheckoutPoliciesService _checkoutPoliciesService;
        private readonly IContentManager _contentManager;

        public CheckoutPoliciesOrderAdditionalInformationProvider(
            ICheckoutPoliciesService checkoutPoliciesService,
            IContentManager contentManager) {

            _checkoutPoliciesService = checkoutPoliciesService;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<XElement> PrepareAdditionalInformation(OrderContext context) {
            // add information on the policies the user had accepted
            var workContext = context.WorkContextAccessor.GetContext();
            var user = workContext.CurrentUser;
            var culture = workContext.CurrentCulture;

            var policies = _checkoutPoliciesService.CheckoutPoliciesForUser(user, culture);

            foreach (var policy in policies) {
                var policyPart = policy.PolicyText != null
                    ? policy.PolicyText
                    : _contentManager.Get<PolicyTextInfoPart>(policy.PolicyTextId);

                var policyXElement = new XElement(CheckoutPolicySettingsPart.OrderXElementName)
                    .With(policy)
                    .ToAttr(p => p.AnswerId)
                    .ToAttr(p => p.PolicyTextId)
                    .ToAttr(p => p.AnswerDate)
                    .ToAttr(p => p.OldAccepted)
                    .ToAttr(p => p.Accepted)
                    .ToAttr(p => p.UserId);
                // we save the "state" of the PolicyTextInfoPart
                policyXElement.Element
                    .AddEl(new XElement("PolicyTextInfoPart")
                            .With(policyPart)
                        .ToAttr(p => p.UserHaveToAccept)
                        .ToAttr(p => p.Priority)
                        .ToAttr(p => p.PolicyType)
                        .ToAttr(p => p.AddPolicyToRegistration));
                // we should also save the version number of the contentitem for the policy
                policyXElement.Element.SetAttributeValue("VersionNumber", policyPart.ContentItem.Version);
                yield return policyXElement;
            }
        }
    }
}