using Laser.Orchard.FidelityGateway.Models;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.FidelityGateway.Services {
    public class FidelityIdentityProvider : IIdentityProvider {
        public KeyValuePair<string, object> GetRelatedId(Dictionary<string, object> context) {
            var result = new KeyValuePair<string, object>("", 0);
            if (context.ContainsKey("Content")) {
                var ci = context["Content"];
                if (ci is ContentItem) {
                    var fidelity = (ci as ContentItem).As<FidelityUserPart>();
                    if (fidelity != null) {
                        if (string.IsNullOrWhiteSpace(fidelity.CustomerId)) {
                            result = new KeyValuePair<string, object>("FidelityCustomerId", fidelity.CustomerId);
                        }
                    }
                }
            }
            return result;
        }
    }
}