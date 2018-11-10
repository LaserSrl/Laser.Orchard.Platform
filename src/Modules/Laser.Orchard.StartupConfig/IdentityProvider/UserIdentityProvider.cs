using Orchard.ContentManagement;
using Orchard.Users.Models;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.IdentityProvider {
    public class UserIdentityProvider : IIdentityProvider {
        public KeyValuePair<string, object> GetRelatedId(Dictionary<string, object> context) {
            var result = new KeyValuePair<string, object>("", 0);
            if (context.ContainsKey("Content")) {
                var ci = context["Content"];
                if (ci is ContentItem) {
                    var user = (ci as ContentItem).As<UserPart>();
                    if(user != null) {
                        result = new KeyValuePair<string, object>("UserId", user.Id);
                    }
                }
            }
            return result;
        }
    }
}