using Laser.Orchard.CommunicationGateway.Helpers;
using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Users.Models;
using System;
using System.Linq;

namespace Laser.Orchard.Mobile.Handlers {
    [OrchardFeature("Laser.Orchard.GDPR.PushGatewayExtension")]
    public class MobilePushPartGDPRHandler : ContentGDPRHandler {

        // This handler is here to take care of the personal identifiable information
        // in MobilePushPart. Specifically, the RecipientList. That property contains
        // a list of email addresses: when we are anonymizing/erasing a user/a contact, 
        // we need to make sure that the corresponding email addresses are gone form 
        // everywhere in our system, including this list. This list may contain either
        // the usernames or the email addresses.
        // There two cases to handle:
        // - We are processing a content that contains the MobilePushPart, in which case
        //   we should wipe everything from the RecipientList in all versions of the content.
        // - We are processing a user, in which case we must remove all references to
        //   their email or username from all versions of all MobilePushPart.

        private readonly IContentManager _contentManager;

        public MobilePushPartGDPRHandler(
            IContentManager contentManager) {

            _contentManager = contentManager;

            OnAnonymizing<UserPart>(HandleUser);
            OnErasing<UserPart>(HandleUser);

            OnAnonymizing<MobilePushPart>(HandleMobilePushPart);
            OnErasing<MobilePushPart>(HandleMobilePushPart);
        }

        private void HandleUser(GDPRContentContext context, UserPart userPart) {
            // Given the user, we should process every MobilePushPart ever, including
            // eventually deleted ones. We should check whether in its RecipientList there
            // is either the username or the email of the current user, and remove it if 
            // that is the case.
            var comparisonArray = new string[] {
                userPart.UserName?.Trim(),
                userPart.Email?.Trim()
            }.Where(str => !string.IsNullOrWhiteSpace(str));

            if (comparisonArray.Any()) { // sanity check
                var mpParts = _contentManager.Query<MobilePushPart>(VersionOptions.AllVersions)
                    .Where<MobilePushPartRecord>(mppr => // this predicate replaces 
                        mppr.RecipientList != null // !string.IsNullOrWhiteSpace(str)
                        && mppr.RecipientList != "")
                    .List() // here the IContentQuery ends and the IEnumerable begins
                    .Where(mpp => {// only those that may contain either username or email
                        foreach (var compare in comparisonArray) {
                            if (mpp.RecipientList
                                .Contains(compare, StringComparison.OrdinalIgnoreCase)) {
                                return true;
                            }
                        }
                        return false;
                        });
                
                foreach (var mpPart in mpParts) {
                    // RecipientList is a string of usernames or email addresses separated by
                    // a new line.
                    var split = mpPart.RecipientList
                        .Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    mpPart.RecipientList = string.Join(Environment.NewLine,
                        split.Select(str => str.Trim()) // get rid of spare whitespace
                            .Except(comparisonArray, StringComparer.OrdinalIgnoreCase));
                }
            }
        }

        private void HandleMobilePushPart(GDPRContentContext context, MobilePushPart mpPart) {
            // clear the RecipientsList in all versions of the content.
            var partVersions = context.AllVersions // all the versions of the ContentItem being processed
                .Select(civ => civ
                    .Parts
                    .FirstOrDefault(pa => pa is MobilePushPart))
                .Where(pa => pa != null)
                .Cast<MobilePushPart>();
            foreach (var part in partVersions) {
                part.RecipientList = string.Empty;
            }
        }
    }
}