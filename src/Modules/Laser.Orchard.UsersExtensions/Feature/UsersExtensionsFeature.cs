using Laser.Orchard.Policy.Services;
using Laser.Orchard.UsersExtensions.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment;
using System.Collections.Generic;

namespace Laser.Orchard.UsersExtensions.Feature {
    public class UsersExtensionsFeature : IFeatureEventHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IPolicyServices _policyServices;
        public UsersExtensionsFeature(IOrchardServices orchardServices, IPolicyServices policyServices) {
            _orchardServices = orchardServices;
            _policyServices = policyServices;
        }
        public void Disabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Disabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Enabled(global::Orchard.Environment.Extensions.Models.Feature feature) {
            var settings = _orchardServices.WorkContext.CurrentSite.As<UserRegistrationSettingsPart>();
            var list = new List<string>();
            var policies = _policyServices.GetAllPublishedPolicyTexts();
            foreach(var p in policies) {
                var currentPolicyId = string.Format("{{{0}}}", p.Id);
                if (p.AddPolicyToRegistration) {
                    list.Add(currentPolicyId);
                }
            }
            settings.IncludePendingPolicy = (list.Count == 0) ? Policy.IncludePendingPolicyOptions.No : Policy.IncludePendingPolicyOptions.Yes;
            settings.PolicyTextReferences = list.ToArray();
        }

        public void Enabling(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Installed(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Installing(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Uninstalled(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }

        public void Uninstalling(global::Orchard.Environment.Extensions.Models.Feature feature) {
        }
    }
}