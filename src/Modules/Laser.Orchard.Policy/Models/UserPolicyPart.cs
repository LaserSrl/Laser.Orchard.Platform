using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Policy.Models {
    public class UserPolicyPart : ContentPart<UserPolicyPartRecord> {

        public IEnumerable<UserPolicyAnswersRecord> UserPolicyAnswers {
            get {
                return Record.UserPolicyAnswers.OrderByDescending(o=>o.PolicyTextInfoPartRecord.Priority).Select(r => r);
            }
        }
    }

    public class UserPolicyPartRecord : ContentPartRecord {

        public UserPolicyPartRecord() {
            UserPolicyAnswers = new List<UserPolicyAnswersRecord>();
        }
        public virtual IList<UserPolicyAnswersRecord> UserPolicyAnswers { get; set; }
    }
}