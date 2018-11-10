using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UsersExtensions {
    public enum LostPasswordUserOptions {
        Account, Phone
    };

    public enum PoliciesRequestType {
        All, ForRegistration, NotForRegistration
    };
}