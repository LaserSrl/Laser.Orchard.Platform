using System;
using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Models;
using Orchard.Localization.Services;

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
                    return GetPendingPolicies(context, part).List<IContent>().Count() > 0;
                });
                part._pendingPolicies.Loader(() => {
                    RealPolicyInclusionSetter(part);
                    if (!IsPolicyIncluded(part)) return null;
                    return GetPendingPolicies(context, part).List<IContent>().Select(s => (IContent)s.ContentItem).ToList();
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
        /// <summary>
        /// Get pending policies: policies for which the user has not yet expressed his opinion
        /// </summary>
        /// <param name="context">Context of the showing content</param>
        /// <param name="part">the PolicyPart that describe which policies should be checked</param>
        /// <returns>The IContentQuery that returns the list of the pending policies</returns>
        private IContentQuery GetPendingPolicies(LoadContentContext context, PolicyPart part) {
            var loggedUser = _workContext.GetContext().CurrentUser;
            int currentLanguageId;
            IContentQuery<PolicyTextInfoPart> query;

            if (context.ContentItem.As<LocalizationPart>() != null && context.ContentItem.As<LocalizationPart>().Culture != null && context.ContentItem.As<LocalizationPart>().Culture.Id > 0) {
                currentLanguageId = context.ContentItem.As<LocalizationPart>().Culture.Id;
            } else {
                //Nel caso di contenuto senza Localizationpart prendo la CurrentCulture
                currentLanguageId = _cultureManager.GetCultureByName(_workContext.GetContext().CurrentCulture).Id;
            }

            query = _contentManager.Query<PolicyTextInfoPart>().Join<LocalizationPartRecord>().Where(w => w.CultureId == currentLanguageId || w.CultureId == 0);

            //recupero solo le Policy Pendenti, alle quali l'utente non ha risposto ancora

            IContentQuery<PolicyTextInfoPart, PolicyTextInfoPartRecord> items;
            if (loggedUser != null) {
                var answeredIds = loggedUser.As<UserPolicyPart>().UserPolicyAnswers.Select(s => s.PolicyTextInfoPartRecord.Id).ToArray();
                items = query.Where<PolicyTextInfoPartRecord>(w => !answeredIds.Contains(w.Id));
            } else {
                IList<PolicyForUserViewModel> answers = _policyServices.GetCookieOrVolatileAnswers();
                var answeredIds = answers.Select(s => s.PolicyTextId).ToArray();
                items = query.Where<PolicyTextInfoPartRecord>(w => !answeredIds.Contains(w.Id));
            }

            var settings = part.Settings.GetModel<PolicyPartSettings>();

            if (!settings.PolicyTextReferences.Contains("{All}")) {

                int[] filterIds;

                string[] filterComplexIds = _policyServices.GetPoliciesForContent(part);

                if (filterComplexIds != null) {
                    if (filterComplexIds.Length == 1) {
                        filterComplexIds = filterComplexIds[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    filterIds = filterComplexIds.Select(s => {
                        int id = 0;
                        int.TryParse(s.Replace("{", "").Replace("}", ""), out id);
                        return id;
                    }).ToArray();

                    items = items.Where<PolicyTextInfoPartRecord>(w => filterIds.Contains(w.Id));
                }
            }

            return items.OrderByDescending(x=>x.Priority);
        }

    }
}