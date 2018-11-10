using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Policy {
    public enum IncludePendingPolicyOptions {
        Yes, No, DependsOnContent
    }

    public enum PolicyTypeOptions {
        Policy, Regulation, CommercialUse, ThirdParty
    }
}