using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.GDPR.Helpers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.CommunicationGateway.Handlers {
    /// <summary>
    /// This handler should take care of cleaning default information that we know we are storing in 
    /// some records from our CommunicationGateway. In particular, we are storing a user's email addresses 
    /// in the CommunicationEmailRecords, and we are storing the user's mobile phone numbers in the
    /// CommunicationSmsRecords.
    /// </summary>
    [OrchardFeature("Laser.Orchard.GDPR.ContactExtension")]
    public class ContactGDPRHandler : ContentGDPRHandler {

        private readonly IRepository<CommunicationEmailRecord> _emailRecordRepository;
        private readonly IRepository<CommunicationSmsRecord> _smsRecordRepository;

        public ContactGDPRHandler(
            IRepository<CommunicationEmailRecord> emailRecordRepository,
            IRepository<CommunicationSmsRecord> smsRecordRepository) {

            _emailRecordRepository = emailRecordRepository;
            _smsRecordRepository = smsRecordRepository;

            // We wipe information in the -ed methods rather than in the -ing methods,
            // to ensure that other handlers are still able to use the values to identify
            // other related stuff to process. See for example the SmsGatewayPartGDPRHandler
            // in the Laser.Orchard.Mobile module.

            // When handling the EmailContactPart we should ensure that we are clearing the
            // email address from all related CommunicationEmailRecords
            OnAnonymized<EmailContactPart>(HandleEmailRecords);
            OnErased<EmailContactPart>(HandleEmailRecords);
            // Similarly, we should remove the mobile phone numbers when we handle the
            // SmsContactPart.
            OnAnonymized<SmsContactPart>(HandleSmsRecords);
            OnErased<SmsContactPart>(HandleSmsRecords);
        }

        // We cannot replace neither the email address nor the mobile phone number with default
        // strings. This is because the drivers will verify that those are unique when editing 
        // the contacts. Any update there would then fail.
        // Thus we will hash a combination of the personal information and the UTC DateTime of the
        // anonymization/erasure. This should ensure that the resulting strings are unique. Since
        // the DateTime of the process may be logged somewhere, to prevent bruteforce discovery of
        // the original string (which would be comparatively simple in the case of the phone number)
        // rather than using a contant key, we generate a new one each time.

        private void HandleEmailRecords(GDPRContentContext context, EmailContactPart part) {
            foreach (var emailRecord in part.EmailRecord) {
                // Clear the email address
                emailRecord.Email = emailRecord.Email.GenerateUniqueString();
                _emailRecordRepository.Update(emailRecord);
            }
        }

        private void HandleSmsRecords(GDPRContentContext context, SmsContactPart part) {
            foreach (var smsRecord in part.SmsRecord) {
                // Clear the phone number
                smsRecord.Sms = smsRecord.Sms.GenerateUniqueString();
                _smsRecordRepository.Update(smsRecord);
            }
        }

    }
}