using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.StartupConfig.Handlers;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Handlers {
    public class ContactRelatedEventHandler : IContactRelatedEventHandler {
        public readonly ICommunicationService _communicationService;

        public ContactRelatedEventHandler(ICommunicationService communicationService) {
            _communicationService = communicationService;
        }

        public void Synchronize() {
            //throw new NotImplementedException();
        }

        public void Synchronize(IUser user) {
            _communicationService.UserToContact(user);
        }

        public void ContactRemoved(int contactId) {
        }
    }
}