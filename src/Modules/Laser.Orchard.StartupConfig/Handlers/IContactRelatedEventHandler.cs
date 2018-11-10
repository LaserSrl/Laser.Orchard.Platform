using Orchard.Events;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Handlers {
    public interface IContactRelatedEventHandler : IEventHandler {
        void Synchronize();
        void Synchronize(IUser user);
        void ContactRemoved(int contactId);
    }
}