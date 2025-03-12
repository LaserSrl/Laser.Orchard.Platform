using Laser.Orchard.Mobile.Services;
using Laser.Orchard.StartupConfig.Handlers;

namespace Laser.Orchard.Mobile.Handlers {
    public class ContactRelatedEventHandler : IContactRelatedEventHandler {
        public readonly IPushNotificationService _pushNotificationservice;

        public ContactRelatedEventHandler(IPushNotificationService pushNotificationservice) {
            _pushNotificationservice = pushNotificationservice;
        }

        public void Synchronize() {
            _pushNotificationservice.Synchronize();
        }

        public void Synchronize(global::Orchard.Security.IUser user) {
        }

        public void ContactRemoved(int contactId) {
            _pushNotificationservice.RebindDevicesToMasterContact(contactId);
        }
    }
}