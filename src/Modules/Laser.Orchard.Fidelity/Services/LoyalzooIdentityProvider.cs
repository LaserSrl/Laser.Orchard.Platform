using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.Fidelity.Services {
    public class LoyalzooIdentityProvider : IIdentityProvider {
        public KeyValuePair<string, object> GetRelatedId(Dictionary<string, object> context) {
            var result = new KeyValuePair<string, object>("", 0);
            var tempData = new List<KeyValuePair<string, bool>>();
            if (context.ContainsKey("Content")) {
                var ci = context["Content"];
                if (ci is ContentItem) {
                    var loyalzoo = (ci as ContentItem).As<LoyalzooUserPart>();
                    if (loyalzoo != null) {
                        if (string.IsNullOrWhiteSpace(loyalzoo.CustomerSessionId)) {
                            tempData.Add(new KeyValuePair<string, bool>("LoyalzooRegistrationSuccess", false));
                        } else {
                            tempData.Add(new KeyValuePair<string, bool>("LoyalzooRegistrationSuccess", true));
                        }
                        result = new KeyValuePair<string, object>("RegisteredServices", tempData);
                    }
                }
            }
            return result;
        }
    }
}