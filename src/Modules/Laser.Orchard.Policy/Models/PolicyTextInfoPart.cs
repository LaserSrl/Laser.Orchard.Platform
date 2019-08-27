using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Policy.Models {

    public class PolicyTextInfoPartRecord : ContentPartRecord {
        public virtual bool UserHaveToAccept { get; set; }
        public virtual int Priority { get; set; }
        public virtual PolicyTypeOptions PolicyType { get; set; }
        public virtual bool AddPolicyToRegistration { get; set; }
    }
    public class PolicyTextInfoPart : ContentPart<PolicyTextInfoPartRecord> {

        [Required]
        public bool UserHaveToAccept {
            get { return this.Retrieve(x => x.UserHaveToAccept); }
            set { this.Store(x => x.UserHaveToAccept, value); }
        }

        [Required]
        public int Priority {
            get { return this.Retrieve(x => x.Priority); }
            set { this.Store(x => x.Priority, value); }
        }

        public PolicyTypeOptions PolicyType {
            get { return this.Retrieve(x => x.PolicyType); }
            set { this.Store(x => x.PolicyType, value); }
        }

        public bool AddPolicyToRegistration {
            get { return this.Retrieve(x => x.AddPolicyToRegistration); }
            set { this.Store(x => x.AddPolicyToRegistration, value); }
        }
    }
}