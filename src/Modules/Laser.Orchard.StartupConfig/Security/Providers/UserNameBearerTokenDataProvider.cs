using Orchard.Environment.Extensions;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Security.Providers {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class UserNameBearerTokenDataProvider : BaseBearerTokenDataProvider {
        public UserNameBearerTokenDataProvider(
            // Inject any required IDependency    
            ) : base(false) {
            // MyImplementation ctor body
        }
        protected override string Value(IUser user) {
            return user.UserName;
        }
        public override string Key {
            get { return "UserName"; }
        }
    }
}