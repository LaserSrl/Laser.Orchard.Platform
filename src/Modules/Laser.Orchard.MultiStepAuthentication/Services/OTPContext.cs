using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    public class OTPContext {
        public IUser User { get; set; }
        public string Password { get; set; }
        public Dictionary<string, string> AdditionalInformation { get; set; }
    }
}