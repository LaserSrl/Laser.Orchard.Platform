
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Laser.Orchard.UsersExtensions.Models;
using System.Linq;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.Policy.Services;

namespace Laser.Orchard.UsersExtensions.Handlers {

    public class UserRegistrationSettingsPartHandler : ContentHandler {
        private readonly IUtilsServices _utilsServices;
        private readonly IPolicyServices _policyServices;
        public UserRegistrationSettingsPartHandler(IUtilsServices utilsServices, IPolicyServices policyServices) {
            _utilsServices = utilsServices;
            _policyServices = policyServices;
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<UserRegistrationSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<UserRegistrationSettingsPart>("UserRegistrationSettings", "Parts/UsersRegistrationSettings", "UserExtras"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Policy")) {
                base.GetItemMetadata(context);
                context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("UserExtras")));
            }
        }

        protected override void Updated(UpdateContentContext context) {
            if (context.ContentItem.ContentType == "Site") {
                var model = context.ContentItem.As<UserRegistrationSettingsPart>();
                if (model.PolicyTextReferences != null && model.PolicyTextReferences.Length > 0) { // rimouovo tutti i dati superflui
                    if (model.PolicyTextReferences == null || model.PolicyTextReferences.Length == 0) {
                        model.PolicyTextReferences = new string[] { "{All}" };
                    } else if (model.PolicyTextReferences.Contains("{All}")) {
                        model.PolicyTextReferences = new string[] { "{All}" };
                    } else if (model.PolicyTextReferences.Contains("{DependsOnContent}")) {
                        model.PolicyTextReferences = new string[] { "{DependsOnContent}" };
                    }
                    context.ContentItem.As<UserRegistrationSettingsPart>().PolicyTextReferences = model.PolicyTextReferences;

                    // get all published policies
                    var policies = _policyServices.GetAllPublishedPolicyTexts();
                    // update policies settings
                    if(model.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.No) {
                        // set all policies to AddPolicyToRegistration = false
                        foreach (var p in policies) {
                            p.AddPolicyToRegistration = false;
                        }
                    } else { // IncludePendingPolicy = Yes
                        if (model.PolicyTextReferences.Contains("{All}")) {
                            // set all policies to AddPolicyToRegistration = true
                            foreach (var p in policies) {
                                p.AddPolicyToRegistration = true;
                            }
                        }
                        else if (model.PolicyTextReferences.Length > 0) {
                            // update all policies
                            foreach (var p in policies) {
                                p.AddPolicyToRegistration = model.PolicyTextReferences.Contains(string.Format("{{{0}}}", p.Id));
                            }
                        }
                    }
                }
                base.Updated(context);
            }
        }
    }
}