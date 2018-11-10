using Laser.Orchard.CommunicationGateway.Helpers;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.Mobile.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Linq;

namespace Laser.Orchard.Mobile.Handlers {
    [OrchardFeature("Laser.Orchard.GDPR.SmsGatewayExtension")]
    public class SmsGatewayPartGDPRHandler : ContentGDPRHandler {

        // This handler is here to take care of personal identifiable information
        // in SmsGatewayPart. Specifically, the RecipientList. This property contains
        // a list of phone numbers: when we are anonymizing/erasing a user/a contact,
        // we need to make sure that the corresponding phone numbers are gone from
        // everywhere in our system, including this list.
        // There are two cases to handle:
        // - We are processing a content that contains the SmsGatewayPart, in which case
        //   we should wipe everything from the RecipientList for all versions of the content.
        // - We are processing a user, in which case we must remove all references to their
        //   phone numbers from all versions of all SmsGatewayPart.

        private readonly IContentManager _contentManager;

        public SmsGatewayPartGDPRHandler(
            IContentManager contentManager) {

            _contentManager = contentManager;

            // SmsGatewayExtension depends on SmsGateway, that in turn depends on 
            // CommunicationGateway.
            // This means that we can handle phone numbers when we are handling the 
            // SmsContactPart there.
            OnAnonymizing<SmsContactPart>(HandleContact);
            OnErasing<SmsContactPart>(HandleContact);

            OnAnonymizing<SmsGatewayPart>(HandlePart);
            OnErasing<SmsGatewayPart>(HandlePart);
        }

        private void HandleContact(GDPRContentContext context, SmsContactPart smsContactPart) {
            // Get all the phone numbers we'll have to process
            var numbers = smsContactPart.SmsRecord
                .Select(csr => new PhoneNumber(csr))
                .Where(pn => !string.IsNullOrWhiteSpace(pn.Number)); // sanity check

            if (numbers.Any()) { // sanity check
                var sgParts = _contentManager.Query<SmsGatewayPart>(VersionOptions.AllVersions)
                    .Where<SmsGatewayPartRecord>(sgpr => // this predicate replaces
                        sgpr.RecipientList != null //!string.IsNullOrWhitespace
                        && sgpr.RecipientList != "")
                    .List() // here IContentQuery ends and IEnumerable begins
                    .Where(sgp => {
                        foreach (var pn in numbers) {
                            if (sgp.RecipientList
                                .Contains(pn.Number, StringComparison.OrdinalIgnoreCase)) {
                                return true;
                            }
                        }
                        return false;
                    });

                // Each phone number in the RecipientLists should be prefix+number, but we check
                // also for the case where it is just number
                var comparisonArray = numbers.Select(pn => pn.Number).ToList();
                comparisonArray.AddRange(numbers.Select(pn => pn.FullNumber));

                foreach (var sgPart in sgParts) {
                    // RecipientList is a string of phone numbers separated by newline.
                    var split = sgPart.RecipientList
                        .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    sgPart.RecipientList = string.Join(Environment.NewLine,
                        split.Select(str => str.Trim())
                            .Except(comparisonArray, StringComparer.OrdinalIgnoreCase));
                }
            }

        }

        private void HandlePart(GDPRContentContext context, SmsGatewayPart smsGatewayPart) {
            // clear the RecipientsList in all versions of the content.
            var partVersions = context.AllVersions // all the versions of the ContentItem
                .Select(civ => civ
                    .Parts
                    .FirstOrDefault(pa => pa is SmsGatewayPart))
                .Where(pa => pa != null)
                .Cast<SmsGatewayPart>();
            foreach (var part in partVersions) {
                part.RecipientList = string.Empty;
            }
        }

        class PhoneNumber {
            public string Prefix { get; set; }
            public string Number { get; set; }

            public string FullNumber { get { return Prefix + Number; } }

            public PhoneNumber(CommunicationSmsRecord record) {
                Prefix = record.Prefix ?? string.Empty;
                Number = record.Sms ?? string.Empty;
            }
        }
    }
}