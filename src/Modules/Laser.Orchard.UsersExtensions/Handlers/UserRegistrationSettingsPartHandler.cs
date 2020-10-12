
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
        public UserRegistrationSettingsPartHandler(
            IUtilsServices utilsServices, 
            IPolicyServices policyServices) {

            _utilsServices = utilsServices;
            _policyServices = policyServices;

            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<UserRegistrationSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<UserRegistrationSettingsPart>(
                "UserRegistrationSettings", "Parts/UsersRegistrationSettings", "UserExtras"));
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
                // we will be here even if we are not updating directly this setting, because site is
                // also being updated elsewhere.
                var model = context.ContentItem.As<UserRegistrationSettingsPart>();
                if (model != null) {
                    if (// if there is nothing selected
                        model.PolicyTextReferences == null 
                        || model.PolicyTextReferences.Length == 0
                        // or we  should show all policies
                        || model.PolicyTextReferences.Contains("{All}")) {
                        // set a default for the array of policies to show
                        model.PolicyTextReferences = new string[] { "{All}" };
                    } else if (model.PolicyTextReferences.Contains("{DependsOnContent}")) {
                        model.PolicyTextReferences = new string[] { "{DependsOnContent}" };
                    }
                    context.ContentItem.As<UserRegistrationSettingsPart>().PolicyTextReferences = model.PolicyTextReferences;
                    // update options for policies
                    var policies = _policyServices.GetAllPublishedPolicyTexts();
                    switch (model.IncludePendingPolicy) {
                        // these two cases fall on the Yes. This is the same behavior that used to
                        // be in place. Note that either case is actually a misconfiguration.
                        case Policy.IncludePendingPolicyOptions.DependsOnContent:
                        default:
                        case Policy.IncludePendingPolicyOptions.Yes:
                            if (model.PolicyTextReferences.Contains("{All}")) {
                                // flag all policies to be shown
                                foreach (var p in policies) {
                                    p.AddPolicyToRegistration = true;
                                }
                            } else {
                                // mark all the policies that are selected in the setting so
                                // that they have the correct value for the flag
                                foreach (var p in policies) {
                                    p.AddPolicyToRegistration = model.PolicyTextReferences.Contains(string.Format("{{{0}}}", p.Id));
                                }
                            }
                            break;
                        case Policy.IncludePendingPolicyOptions.No:
                            // make sure each policy is flagged to not be shown
                            foreach (var p in policies) {
                                p.AddPolicyToRegistration = false;
                            }
                            break;
                    }
                }
                base.Updated(context);
            }
        }
    }
}