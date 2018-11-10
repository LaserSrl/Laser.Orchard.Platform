using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard.ContentManagement;
using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Events {

    public interface ICommunicationEventHandler : IEventHandler {
        void PopulateChannel(ContentItem ci, Advertising adv);
    }
}