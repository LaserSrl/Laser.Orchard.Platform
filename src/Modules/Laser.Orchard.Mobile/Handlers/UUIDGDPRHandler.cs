using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.GDPR.Helpers;
using Laser.Orchard.Mobile.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Users.Models;
using System.Linq;

namespace Laser.Orchard.Mobile.Handlers {
    [OrchardFeature("Laser.Orchard.GDPR.MobileExtension")]
    public class UUIDGDPRHandler : ContentGDPRHandler {

        // The UUID can uniquely identify a device, and thus allows figuring out the user.
        // We are storing that information in several places:
        // LatestUUIDForUserRecord: here we store at most one UUID per user, that one being
        //   the one they had on their last login. See LastDeviceUserDataProvider. When the
        //   User related to it is being anonymized/erased, we should clear the UUID.
        // UserDeviceRecord: there may be several of these for each user, each storing the UUID
        //   for a different device. When the User related to them is being anonymized/erased,
        //   we should clear all UUIDs.
        // PushNotificationRecord: a list of this kind of record is in the MobileContactPart.
        //   Each record stores the UUID for a device along with information useful for delivering
        //   push notifications to the device itself.

        private readonly IRepository<LatestUUIDForUserRecord> _latestUUIDForUserRepository;
        private readonly IRepository<UserDeviceRecord> _userDeviceRepository;
        private readonly IRepository<PushNotificationRecord> _pushNotificationRepository;

        public UUIDGDPRHandler(
            IRepository<LatestUUIDForUserRecord> latestUUIDForUserRepository,
            IRepository<UserDeviceRecord> userDeviceRepository,
            IRepository<PushNotificationRecord> pushNotificationRepository) {

            _latestUUIDForUserRepository = latestUUIDForUserRepository;
            _userDeviceRepository = userDeviceRepository;
            _pushNotificationRepository = pushNotificationRepository;

            // LatestUUIDForUserRecord
            OnAnonymizing<UserPart>(HandleLatestUUID);
            OnErasing<UserPart>(HandleLatestUUID);

            // UserDeviceRecord
            OnAnonymizing<UserPart>(HandleDeviceRecords);
            OnErasing<UserPart>(HandleDeviceRecords);

            // PushNotificationRecord
            OnAnonymizing<MobileContactPart>(HandlePushNotificationRecords);
            OnErasing<MobileContactPart>(HandlePushNotificationRecords);
        }

        private void HandleLatestUUID(GDPRContentContext context, UserPart part) {
            // the record may not exist
            var userRecord = part.Record;
            if (userRecord != null) { // sanity check
                var latestRecord = _latestUUIDForUserRepository
                    .Fetch(luu => luu.UserPartRecord == userRecord)
                    .FirstOrDefault();
                if (latestRecord != null) {
                    // we had recorded the UUID, so now we clear it
                    latestRecord.UUID = latestRecord.UUID.GenerateUniqueString();
                    _latestUUIDForUserRepository.Update(latestRecord);
                }
            }
        }

        private void HandleDeviceRecords(GDPRContentContext context, UserPart part) {

            var userRecord = part.Record;
            if (userRecord != null) { // sanity check
                var deviceRecords = _userDeviceRepository
                    .Fetch(ud => ud.UserPartRecord == userRecord)
                    .ToList();
                foreach (var deviceRecord in deviceRecords) {
                    // clear this UUID
                    deviceRecord.UUIdentifier = deviceRecord.UUIdentifier.GenerateUniqueString();
                    _userDeviceRepository.Update(deviceRecord);
                    // These records we are processing here will be deleted elsewhere if
                    // the user is deleted. However, here we cannot know if the user is going
                    // to be deleted, and we cannot assume it will be. For this reason we
                    // are putting the unique string there.
                }
            }
        }

        private void HandlePushNotificationRecords(GDPRContentContext context, MobileContactPart part) {
            foreach (var pnRecord in part.MobileRecord) {
                pnRecord.UUIdentifier = pnRecord.UUIdentifier.GenerateUniqueString();
                _pushNotificationRepository.Update(pnRecord);
            }
        }
    }
}