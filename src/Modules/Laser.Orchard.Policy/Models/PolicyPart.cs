using System;
using System.Linq;
using Orchard.ContentManagement;

namespace Laser.Orchard.Policy.Models
{
    public class PolicyPart : ContentPart {
        public IncludePendingPolicyOptions IncludePendingPolicy {
            get { return this.Retrieve(x => x.IncludePendingPolicy); }
            set { this.Store(x => x.IncludePendingPolicy, value); }
        }

        public string PolicyTextReferencesCsv {
            get { return this.Retrieve(x => x.PolicyTextReferencesCsv); }
            set { this.Store(x => x.PolicyTextReferencesCsv, value); }
        }

        public string[] PolicyTextReferences {
            get {
                return this.PolicyTextReferencesCsv != null 
                    ? this.PolicyTextReferencesCsv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) 
                    : null;
            }
            set {
                if (value.Contains("{All}")) {
                    this.PolicyTextReferencesCsv = "{All}";
                } else {
                    this.PolicyTextReferencesCsv = String.Join(",", value);
                }
            }
        }
    }
}