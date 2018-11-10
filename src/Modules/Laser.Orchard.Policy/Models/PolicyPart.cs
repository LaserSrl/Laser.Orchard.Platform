using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Laser.Orchard.Policy.Models {
    public class PolicyPart : ContentPart {

        internal LazyField<bool?> _hasPendingPolicies = new LazyField<bool?>();
        internal LazyField<IList<IContent>> _pendingPolicies = new LazyField<IList<IContent>>();

        public IncludePendingPolicyOptions IncludePendingPolicy {
            get { return this.Retrieve(x => x.IncludePendingPolicy); }
            set { this.Store(x => x.IncludePendingPolicy, value); }
        }

        public string PolicyTextReferencesCsv {
            get { return this.Retrieve(x => x.PolicyTextReferencesCsv); }
            set { this.Store(x => x.PolicyTextReferencesCsv, value); }
        }

        public string[] PolicyTextReferences {
            get { return this.PolicyTextReferencesCsv != null ? this.PolicyTextReferencesCsv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null; }
            set {
                if (value.Contains("{All}")) {
                    this.PolicyTextReferencesCsv = "{All}";
                } else {
                    this.PolicyTextReferencesCsv = String.Join(",", value);
                }
            }
        }

        public bool? HasPendingPolicies {
            get {
                return _hasPendingPolicies.Value;
            }
        }
        public IList<IContent> PendingPolicies {
            get {
                return _pendingPolicies.Value;
            }
        }
    }
}