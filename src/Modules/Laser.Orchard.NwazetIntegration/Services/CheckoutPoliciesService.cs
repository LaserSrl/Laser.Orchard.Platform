using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class CheckoutPoliciesService 
        : ICheckoutPoliciesService, ICheckoutExtensionProvider {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IPolicyServices _policyServices;
        private readonly dynamic _shapeFactory;

        public CheckoutPoliciesService(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals,
            IPolicyServices policyServices,
            IShapeFactory shapeFactory) {

            _workContextAccessor = workContextAccessor;
            _cacheManager = cacheManager;
            _signals = signals;
            _policyServices = policyServices;
            _shapeFactory = shapeFactory;

            _checkoutPoliciesByLanguage = new Dictionary<string, IEnumerable<PolicyTextInfoPart>>();
        }

        // dictionary of required policies for each language
        // to memorize partial results and prevent refetching them
        private Dictionary<string, IEnumerable<PolicyTextInfoPart>> _checkoutPoliciesByLanguage;

        #region ICheckoutPoliciesService
        public bool UserHasAllAcceptedPolicies(IUser user = null, string culture = null) {

            var policiesForUser = CheckoutPoliciesForUser(user, culture);
            if (!policiesForUser.Any()) {
                // there is no policy configured for checkout
                return true;
            }
            // return false if any of the policies should have been accepted
            // and hasn't been.
            return !policiesForUser.Any(pvm => pvm.PolicyText.UserHaveToAccept && !pvm.Accepted);
        }
        #endregion

        #region ICheckoutExtensionProvider
        public IEnumerable<dynamic> AdditionalCheckoutStartShapes() {
            // The shape from this will add the option for the user to accept 
            // checkout policies.
            // There will be one "line" (with a checkbox) for each configured
            // policy that the user hasn't accepted yet.
            // If the user has already accepted a given policy, that line 
            // will not show.

            // The shape will need:
            // - The list of all required policies that are configured for checkout.
            //   - Accepted vs to accept
            // - The list of all optional policies that are configured for checkout.
            //   - Accepted vs to accept
            // These four lists, added up together:
            var allCheckoutPolicies = CheckoutPoliciesForUser();

            yield return _shapeFactory.CheckoutPoliciesCheckoutStartShape(
                AllCheckoutPolicies: allCheckoutPolicies
                );
        }
        #endregion

        #region private methods
        private IEnumerable<PolicyForUserViewModel> CheckoutPoliciesForUser(IUser user = null, string culture = null) {
            var settings = GetSettings();
            if (settings.PolicyTextReferences.Contains(CheckoutPolicySettingsPart.NoPolicyOption)) {
                // no policy is configured for checkout
                return Enumerable.Empty<PolicyForUserViewModel>();
            }
            // check the user and culture
            var workContext = _workContextAccessor.GetContext();
            var userToCheck = user ?? workContext.CurrentUser;
            var cultureToCheck = string.IsNullOrWhiteSpace(culture)
                ? workContext.CurrentCulture : culture;
            // get checkout policies for the language (if any)
            if (!_checkoutPoliciesByLanguage.ContainsKey(cultureToCheck)) {
                // fetch the required policies for the language
                _checkoutPoliciesByLanguage.Add(
                    cultureToCheck,
                    CheckoutPoliciesForCulture(cultureToCheck));
            }
            var checkoutPolicies = _checkoutPoliciesByLanguage[cultureToCheck];
            if (!checkoutPolicies.Any()) {
                // no policy is configured for checkout
                return Enumerable.Empty<PolicyForUserViewModel>();
            }
            // State of all the site's policies for the user. This includes the ones
            // for which the user hasn't given an answer yet.
            var userPolicies = _policyServices
                .GetPoliciesForUserOrSession(userToCheck, false, cultureToCheck)
                .Policies;
            // out of those, only return the ones that are configured for checkout
            return userPolicies
                .Where(up => checkoutPolicies.Any(cp => cp.Id == up.PolicyText.Id));
        }

        private CheckoutPolicySettingsPart GetSettings() {
            return _cacheManager.Get(CheckoutPolicySettingsPart.CacheKey,
                ctx => {
                    ctx.Monitor(_signals.When(CheckoutPolicySettingsPart.CacheKey));
                    var settingsPart = _workContextAccessor.GetContext()
                        .CurrentSite.As<CheckoutPolicySettingsPart>();
                    return settingsPart;
                });
        }

        private IEnumerable<PolicyTextInfoPart> CheckoutPoliciesForCulture(string culture) {
            return _policyServices.GetPolicies(culture, CheckoutPolicyIds());
        }

        private int[] _checkoutPolicyIds;
        private int[] CheckoutPolicyIds() {
            if (_checkoutPolicyIds == null) {
                var settings = GetSettings();
                if (settings.PolicyTextReferences.Contains(CheckoutPolicySettingsPart.AllPoliciesOption)) {
                    // _policyServices.GetPolicies uses the null value as indication
                    // that any Id is fine
                    return null;
                }
                if (settings.PolicyTextReferences.Contains(CheckoutPolicySettingsPart.NoPolicyOption)) {
                    return new int[] { };
                }
                // actually parse the Ids
                _checkoutPolicyIds = settings.PolicyTextReferences
                    .Select(s => {
                        var reference = s.Trim(new char[] { '{', '}' });
                        int id;
                        if (int.TryParse(reference, out id)) {
                            return id;
                        }
                        return 0;
                    })
                    .Where(i => i > 0)
                    .ToArray();
            }
            return _checkoutPolicyIds;
        }
        #endregion


    }
}