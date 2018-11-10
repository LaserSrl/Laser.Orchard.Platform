using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Policy.ViewModels {
    public class PolicyEventViewModel {
        public PolicyTypeOptions policyType { get; set; }
        public bool accepted { get; set; }
        public int ItemPolicyPartRecordId { get; set; }
    }
}