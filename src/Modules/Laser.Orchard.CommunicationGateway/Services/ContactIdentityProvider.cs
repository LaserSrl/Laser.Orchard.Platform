using Laser.Orchard.StartupConfig.IdentityProvider;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Services {
    public class ContactIdentityProvider : IIdentityProvider {
        private readonly ICommunicationService _communicationservice;
        public ContactIdentityProvider(ICommunicationService communicationservice) {
            _communicationservice = communicationservice;
        }
        public KeyValuePair<string, object> GetRelatedId(Dictionary<string, object> context) {
            var result = new KeyValuePair<string, object>("", 0);
            if (context.ContainsKey("Content")) {
                var ci = context["Content"];
                if (ci is ContentItem) {
                    var user = (ci as ContentItem).As<UserPart>();
                    if (user != null) {
                        var contact = _communicationservice.GetContactFromUser(user.Id);
                        if(contact != null) {
                            result = new KeyValuePair<string, object>("ContactId", contact.Id);
                        }
                    }
                }
            }
            return result;
        }
    }
}