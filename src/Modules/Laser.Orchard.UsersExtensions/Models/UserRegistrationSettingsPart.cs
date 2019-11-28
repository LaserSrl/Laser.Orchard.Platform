using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Policy;
using Orchard.ContentManagement;

namespace Laser.Orchard.UsersExtensions.Models {
    public class UserRegistrationSettingsPart : ContentPart {
        public IncludePendingPolicyOptions IncludePendingPolicy {
            get { return this.Retrieve(x => x.IncludePendingPolicy); }
            set { this.Store(x => x.IncludePendingPolicy, value); }
        }
        public string[] PolicyTextReferences {
            get {
                return (Retrieve<string>("PolicyTextReferences") ?? "")
                    .Split(new[] { ',', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);
            }
            set { Store("PolicyTextReferences", String.Join(", ", value)); }
        }
    }  
}