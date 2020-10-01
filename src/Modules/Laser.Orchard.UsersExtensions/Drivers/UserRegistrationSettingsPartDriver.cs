using Laser.Orchard.Policy;
using Laser.Orchard.UsersExtensions.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UsersExtensions.Drivers {
    /// <summary>
    /// This driver handles import/export for the settings
    /// </summary>
    public class UserRegistrationSettingsPartDriver : ContentPartDriver<UserRegistrationSettingsPart> {
        private readonly IContentManager _contentManager;

        public UserRegistrationSettingsPartDriver(
            IContentManager contentManager) {

            _contentManager = contentManager;
        }


        protected override void Exporting(UserRegistrationSettingsPart part, ExportContentContext context) {
            var element = context.Element(part.PartDefinition.Name);
            element.SetAttributeValue("IncludePendingPolicy", part.IncludePendingPolicy);
            // PolicyTextReferences is a string[]. Each element of the array is either
            // "{All}", "{DependsOnContent}" or "{[int]}".
            // The first two cases can be exported as they are. The latter case is the id of
            // a selected content item, and its identity should be exported instead.
            // In the former case, all policies are considered to be selected. In the latter,
            // we should select the content items with those ids.
            var textReferences = new List<string>();
            if (part.PolicyTextReferences != null && part.PolicyTextReferences.Any()) {
                // in principle, both "All" and "DependsOnContent" may be the identities of contents.
                // As a consequence, it's not safe to just put them as they are in the exported string.
                // The easiest (and wrong) way to export things here would be to have those values in
                // the exported string if they are in the original array. However, we would have no way
                // to know whether those are the exported values or some content item's identities.
                // As a solution, the first two values in the exported string will always be there, and
                // they will be booleans telling whether {All} or {DependsOnContent} were selected.
                textReferences.Add(
                    $"{{{part.PolicyTextReferences.Contains("{All}").ToString()}}}");
                textReferences.Add(
                    $"{{{part.PolicyTextReferences.Contains("{DependsOnContent}").ToString()}}}");
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
                textReferences.Add("{False}");
            }
            element.SetAttributeValue("PolicyTextReferences", string.Join(",", textReferences));
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

        protected override void Importing(UserRegistrationSettingsPart part, ImportContentContext context) {
            var partName = part.PartDefinition.Name;
            var root = context.Data.Element(partName);
            if (root == null) {
                return;
            }
            // enum
            var includePending = context.Attribute(partName, "IncludePendingPolicy");
            var ipp = IncludePendingPolicyOptions.Yes; // default value
            if (Enum.TryParse(includePending, out ipp)) {
                part.IncludePendingPolicy = ipp;
            } else {
                part.IncludePendingPolicy = IncludePendingPolicyOptions.Yes;
            }
            // list of policies
            var references = context.Attribute(partName, "PolicyTextReferences")
                .Split(new char[] { '{', '}', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (references == null || references.Length < 2
                || references[0].Equals("True", StringComparison.InvariantCultureIgnoreCase)) {
                part.PolicyTextReferences = new string[] { "{All}" };
            } else if (references[1].Equals("True", StringComparison.InvariantCultureIgnoreCase)) {
                part.PolicyTextReferences = new string[] { "{DependsOnContent}" };
            } else {
                // parse everything else
                part.PolicyTextReferences = references
                    .Skip(2) // both All and DependsOnContent are false
                    .Select(r => $"{{{(context.GetItemFromSession(r) == null ? string.Empty : context.GetItemFromSession(r).Id.ToString())}}}")
                    .ToArray();
            }
        }
    }
}