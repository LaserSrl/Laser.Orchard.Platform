using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Policy.Handlers {

    public class PolicyPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContext;
        private readonly ICultureManager _cultureManager;
        private readonly IPolicyServices _policyServices;

        public PolicyPartHandler(IContentManager contentManager, IWorkContextAccessor workContext, ICultureManager cultureManager, IPolicyServices policyServices) {
            _contentManager = contentManager;
            _workContext = workContext;
            _cultureManager = cultureManager;
            _policyServices = policyServices;
            OnLoaded<PolicyPart>((context, part) => {
                part._hasPendingPolicies.Loader(() => {
                    RealPolicyInclusionSetter(part);
                    if (!IsPolicyIncluded(part)) return null;
                    return GetPendingPolicies(context, part)
                        .Any();
                });
                part._pendingPolicies.Loader(() => {
                    RealPolicyInclusionSetter(part);
                    if (!IsPolicyIncluded(part)) return null;
                    return GetPendingPolicies(context, part)
                        .Select(s => (IContent)s.ContentItem)
                        .ToList();
                });
            });
        }

        private bool IsPolicyIncluded(PolicyPart part) {
            var settings = part.Settings.GetModel<PolicyPartSettings>();
            if (settings.IncludePendingPolicy == IncludePendingPolicyOptions.Yes) {
                return true;
            } else if (settings.IncludePendingPolicy == IncludePendingPolicyOptions.DependsOnContent && part.IncludePendingPolicy == IncludePendingPolicyOptions.Yes) {
                return true;
            }
            return false;
        }

        private void RealPolicyInclusionSetter(PolicyPart part) {
            var settings = part.Settings.GetModel<PolicyPartSettings>();
            if (settings.IncludePendingPolicy != IncludePendingPolicyOptions.DependsOnContent) {
                part.IncludePendingPolicy = settings.IncludePendingPolicy;
            }
        }
        
        private IEnumerable<IContent> GetPendingPolicies(LoadContentContext context, PolicyPart part) {
            var loggedUser = _workContext.GetContext().CurrentUser;

            // get the name of a culture to pass to find policies
            string cultureName = null;
            if (context.ContentItem.As<LocalizationPart>() != null
                && context.ContentItem.As<LocalizationPart>().Culture != null
                && context.ContentItem.As<LocalizationPart>().Culture.Id > 0) {

                cultureName = context.ContentItem.As<LocalizationPart>().Culture.Culture;
            } else {
                //Nel caso di contenuto senza Localizationpart prendo la CurrentCulture
                cultureName = _workContext.GetContext().CurrentCulture;
            }
            var policies = _policyServices.GetPolicies(cultureName);
            // figure out which policies the user has not answered
            var answeredIds = loggedUser != null
                ? loggedUser
                    .As<UserPolicyPart>().UserPolicyAnswers.Select(s => s.PolicyTextInfoPartRecord.Id)
                : _policyServices.GetCookieOrVolatileAnswers()
                    .Select(s => s.PolicyTextId);
            var items = policies.Where(p => !answeredIds.Contains(p.Id));

            var settings = part.Settings.GetModel<PolicyPartSettings>();
            if (!settings.PolicyTextReferences.Contains("{All}")) {
                string[] filterComplexIds = _policyServices.GetPoliciesForContent(part);
                if (filterComplexIds != null) {
                    if (filterComplexIds.Length == 1) {
                        filterComplexIds = filterComplexIds[0]
                            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    var filterIds = filterComplexIds.Select(s => {
                        int id = 0;
                        int.TryParse(s.Replace("{", "").Replace("}", ""), out id);
                        return id;
                    }).ToArray();

                    items = items.Where(p => filterIds.Contains(p.Id));
                }
            }

            return items;
        }
    }
}