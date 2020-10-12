using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.UsersExtensions.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.UsersExtensions.Handlers {
    public class PolicyTextInfoEventHandler : ContentHandler {

        private readonly IPolicyServices _policyServices;

        public PolicyTextInfoEventHandler(
            IOrchardServices orchardServices,
            IPolicyServices policyServices) {

            _policyServices = policyServices;

            OnPublished<PolicyTextInfoPart>((context, part) => {
                PolicyTextInfoPublished(part, orchardServices);
            });
        }

        private void PolicyTextInfoPublished(
            PolicyTextInfoPart policyTextInfo, 
            IOrchardServices orchardServices) {

            var settings = orchardServices.WorkContext.CurrentSite.As<UserRegistrationSettingsPart>();
            var list = new List<string>(settings.PolicyTextReferences);
            var currentPolicyId = string.Format("{{{0}}}", policyTextInfo.Id);
            var all = "{All}";
            var willIncludePolicies = settings.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.Yes;
            switch (settings.IncludePendingPolicy) {
                case Policy.IncludePendingPolicyOptions.Yes:
                    // policies should be added to the list in the settings if marked
                    // for use during registration, removed if explicitly set to not
                    // be used for registration.
                    if (policyTextInfo.AddPolicyToRegistration) {
                        // add it to the list, unless it's already there, or the list is
                        // explicitly set to include all policies
                        if (!list.Contains(all) && !list.Contains(currentPolicyId)) {
                            list.Add(currentPolicyId);
                        }
                    } else {
                        // remove it from the list
                        if (list.Contains(all)) {
                            // we were adding all policies, but not anymore
                            list.Remove(all);
                            // make sure that all policies that are still explicitly flagged to
                            // be used for registration are in the final list
                            var items = _policyServices
                                .GetAllPublishedPolicyTexts()
                                .Where(pt => pt.AddPolicyToRegistration);
                            foreach (var item in items) {
                                if (!list.Contains(string.Format("{{{0}}}", item.Id)) && item.Id != policyTextInfo.Id) {
                                    list.Add(string.Format("{{{0}}}", item.Id));
                                }
                            }
                        }
                        if (list.Contains(currentPolicyId)) {
                            list.Remove(currentPolicyId);
                        }
                        willIncludePolicies = list.Any();
                    }
                    break;
                case Policy.IncludePendingPolicyOptions.No:
                    if (policyTextInfo.AddPolicyToRegistration) {
                        // we add the id to the list
                        if (list.Contains(all)) {
                            // make sure we won't enable all policies by mistake
                            list.Remove(all);
                        }
                        // the list should only contain those policies that are explicitly marked
                        // to be used for registration. 
                        var items = _policyServices
                            .GetAllPublishedPolicyTexts()
                                .Where(pt => pt.AddPolicyToRegistration);
                        // Recreate it rather than check for each item.
                        list = new List<string>(items
                            .Select(pt => string.Format("{{{0}}}", pt.Id)));
                        if (!list.Contains(currentPolicyId)) {
                            // prevent adding it twice
                            list.Add(currentPolicyId);
                        }
                        // then we make sure policies will actually be included by setting this
                        willIncludePolicies = true;
                    } else {
                        // remove it from the list if it's there
                        if (list.Contains(currentPolicyId)) {
                            list.Remove(currentPolicyId);
                        }
                        // we should not change the value of settings.IncludePendingPolicy 
                    }
                    break;
                    // These two following cases are misconfigurations, probably
                    // TODO: figure them out if we ever need them
                case Policy.IncludePendingPolicyOptions.DependsOnContent:
                    break;
                default:
                    break;
            }
            // finally update settings
            settings.IncludePendingPolicy = willIncludePolicies
                ? Policy.IncludePendingPolicyOptions.Yes : Policy.IncludePendingPolicyOptions.No;
            settings.PolicyTextReferences = list.ToArray();
        }
    }
}