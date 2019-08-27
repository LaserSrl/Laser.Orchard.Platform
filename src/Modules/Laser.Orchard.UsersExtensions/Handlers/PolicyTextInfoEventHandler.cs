using Laser.Orchard.Policy.Models;
using Laser.Orchard.UsersExtensions.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using System.Collections.Generic;

namespace Laser.Orchard.UsersExtensions.Handlers {
    public class PolicyTextInfoEventHandler : ContentHandler {
        public PolicyTextInfoEventHandler(IOrchardServices orchardServices) {
            OnPublished<PolicyTextInfoPart>((context, part) => {
                PolicyTextInfoPublished(part, orchardServices);
            });
        }
        private void PolicyTextInfoPublished(PolicyTextInfoPart policyTextInfo, IOrchardServices orchardServices) {
            var settings = orchardServices.WorkContext.CurrentSite.As<UserRegistrationSettingsPart>();
            var list = new List<string>(settings.PolicyTextReferences);
            var currentPolicyId = string.Format("{{{0}}}", policyTextInfo.Id);
            var all = "{All}";
            if (policyTextInfo.AddPolicyToRegistration) {
                if(list.Contains(all) == false && list.Contains(currentPolicyId) == false) {
                    list.Add(currentPolicyId);
                }
            }
            else { // remove current policy from registration
                if(list.Contains(all)) {
                    list.Remove(all);
                    // insert all other policies
                    var items = orchardServices.ContentManager.List<PolicyTextInfoPart>(new string[] { "PolicyText" });
                    foreach(var item in items) {
                        if(list.Contains(string.Format("{{{0}}}", item.Id)) == false && item.Id != policyTextInfo.Id) {
                            list.Add(string.Format("{{{0}}}", item.Id));
                        }
                    }
                }
                if (list.Contains(currentPolicyId)) {
                    list.Remove(currentPolicyId);
                }
            }
            settings.IncludePendingPolicy = (list.Count == 0) ? Policy.IncludePendingPolicyOptions.No : Policy.IncludePendingPolicyOptions.Yes;
            settings.PolicyTextReferences = list.ToArray();
        }
    }
}