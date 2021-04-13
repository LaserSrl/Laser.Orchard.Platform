using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Controllers;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class CheckoutPolicySettingsPartDriver 
        : ContentPartDriver<CheckoutPolicySettingsPart> {
        private readonly IContentManager _contentManager;

        public CheckoutPolicySettingsPartDriver(
            IContentManager contentManager) {

            _contentManager = contentManager;
        }

        protected override string Prefix => "CheckoutPolicySettingsPart";

        protected override DriverResult Editor(CheckoutPolicySettingsPart part, dynamic shapeHelper) {
            return EditorShape(
                CreateVM(part),
                shapeHelper);
        }

        protected override DriverResult Editor(CheckoutPolicySettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater is ECommerceSettingsAdminController) {
                var vm = new CheckoutPolicySettingsPartEditViewModel();
                if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                    if ( // if there is nothing selected
                        vm.PolicyTextReferences == null
                        || vm.PolicyTextReferences.Length == 0
                        // or we should show all policies
                        || vm.PolicyTextReferences.Contains(CheckoutPolicySettingsPart.NoPolicyOption)) {

                        // no policy required
                        part.PolicyTextReferences = new string[] { CheckoutPolicySettingsPart.NoPolicyOption };
                    } else if (vm.PolicyTextReferences.Contains(CheckoutPolicySettingsPart.AllPoliciesOption)) {
                        // user must accept all policies
                        part.PolicyTextReferences = new string[] { CheckoutPolicySettingsPart.AllPoliciesOption };
                    } else {
                        part.PolicyTextReferences = vm.PolicyTextReferences;
                    }
                }
                return EditorShape(vm, shapeHelper);
            }
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(CheckoutPolicySettingsPart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            // PolicyTextReferences is a string[]. Each element of the array is either
            // "{All}" or "{None}" or "{[int]}".
            // The first two cases can be exported as they are. The latter case is the id of
            // a selected content item, and its identity should be exported instead.
            // In the former cases, all policies are considered to be selected (or none). In the latter,
            // we should select the content items with those ids.
            var textReferences = new List<string>();
            if (part.PolicyTextReferences != null && part.PolicyTextReferences.Any()) {
                // in principle, "All" and "None" may be the identities of ContentItems.
                // As a consequence, it's not safe to just put them as they are in the exported string.
                // The easiest (and wrong) way to export things here would be to have that value in
                // the exported string if it is in the original array. However, we would have no way
                // to know whether that is the exported value or some content item's identity.
                // As a solution, the first values in the exported string will always be there, and
                // it will be booleans telling whether {All} or {None} were selected.
                textReferences.Add(
                    $"{{{part.PolicyTextReferences.Contains(CheckoutPolicySettingsPart.NoPolicyOption).ToString()}}}");
                textReferences.Add(
                    $"{{{part.PolicyTextReferences.Contains(CheckoutPolicySettingsPart.AllPoliciesOption).ToString()}}}");
                // Now we add the identities of all content items found selected
                var identities = ItemIdentities(
                    part.PolicyTextReferences
                    // clean away the { and } around the value
                    .Select(re => re.Trim(new char[] { '{', '}' })));
                textReferences
                    .AddRange(
                        identities
                            .Select(i => $"{{{i.ToString()}}}"));
            } else {
                // by default we consider {All} to be the selected reference.
                textReferences.Add("{True}");
            }
            element.SetAttributeValue("PolicyTextReferences", string.Join(",", textReferences));
        }

        protected override void Importing(CheckoutPolicySettingsPart part, ImportContentContext context) {
            var partName = part.PartDefinition.Name;
            var root = context.Data.Element(partName);
            if (root == null) {
                return;
            }
            var references = context.Attribute(partName, "PolicyTextReferences")
                .Split(new char[] { '{', '}', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (references == null || references.Length <= 1
                || references[0].Equals("True", StringComparison.InvariantCultureIgnoreCase)) {
                // by default, no policies are selected
                // also true if we are importing the "None" option as selected
                part.PolicyTextReferences = new string[] { CheckoutPolicySettingsPart.NoPolicyOption };
            } else if (references[1].Equals("True", StringComparison.InvariantCultureIgnoreCase)) {
                // we are importing the "All" option as selected
                part.PolicyTextReferences = new string[] { CheckoutPolicySettingsPart.AllPoliciesOption };
            } else {
                // parse everything else
                part.PolicyTextReferences = references
                    .Skip(1) // "All" is false
                    .Select(r => $"{{{(context.GetItemFromSession(r) == null ? string.Empty : context.GetItemFromSession(r).Id.ToString())}}}")
                    .ToArray();
            }
        }

        private CheckoutPolicySettingsPartEditViewModel CreateVM(
            CheckoutPolicySettingsPart part) {
            // get all configured PolicyText ContentItems
            var items = _contentManager.List<TitlePart>(new string[] { "PolicyText" });

            return new CheckoutPolicySettingsPartEditViewModel(part) {
                PolicyTitles = items.ToList()
            };
        }

        private DriverResult EditorShape(CheckoutPolicySettingsPartEditViewModel vm, dynamic shapeHelper) {
            return ContentShape("SiteSettings_CheckoutPolicySettings",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "SiteSettings/CheckoutPolicySettings",
                    Model: vm,
                    Prefix: Prefix
                    )).OnGroup("ECommerceSiteSettings");
        }
        
        private IEnumerable<ContentIdentity> ItemIdentities(IEnumerable<string> references) {
            foreach (var reference in references) {
                int id;
                if (int.TryParse(reference, out id)) {
                    var identity = ItemIdentity(id);
                    if (identity != null) {
                        yield return identity;
                    }
                }
            }
            yield break;
        }

        private ContentIdentity ItemIdentity(int id) {
            var item = _contentManager.Get(id);
            // only items of type PolicyText
            if (item.ContentType == "PolicyText") {
                return ItemIdentity(item);
            }
            return null;
        }

        private ContentIdentity ItemIdentity(ContentItem item) {
            return _contentManager.GetItemMetadata(item).Identity;
        }

    }
}