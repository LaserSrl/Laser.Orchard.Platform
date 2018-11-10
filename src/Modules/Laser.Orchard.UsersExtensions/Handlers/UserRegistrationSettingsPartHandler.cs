
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Laser.Orchard.UsersExtensions.Models;
using System.Linq;
using System;
using Laser.Orchard.StartupConfig.Services;

namespace Laser.Orchard.UsersExtensions.Handlers {
    
    public class UserRegistrationSettingsPartHandler : ContentHandler {
        private readonly IUtilsServices _utilsServices;
        public UserRegistrationSettingsPartHandler(IUtilsServices utilsServices) {
            _utilsServices = utilsServices;
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

        //protected override void BuildEditorShape(BuildEditorContext context) {
        //    if (context.ContentItem.ContentType == "Site") {
        //        var model = context.ContentItem.As<UserRegistrationSettingsPart>();
        //        if (model.PolicyTextReferences != null && model.PolicyTextReferences.Length > 0) {
        //            context.ContentItem.As<UserRegistrationSettingsPart>().PolicyTextReferences = model.PolicyTextReferences[0].Split(',');
        //        }
        //        base.BuildEditorShape(context);
        //    }
        //}

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
                }
            }
            base.Updated(context);
        }
    }
}