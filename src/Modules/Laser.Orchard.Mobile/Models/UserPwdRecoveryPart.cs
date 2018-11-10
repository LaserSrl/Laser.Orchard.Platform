using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Models {
    [OrchardFeature("Laser.Orchard.Sms")]
    public class UserPwdRecoveryPart : ContentPart<UserPwdRecoveryPartRecord> {

        [RegularExpression("^(\\+)?\\d{1,3}$", ErrorMessage = "Invalid phone prefix. Only digits accepted.")]
        public string InternationalPrefix {
            get {
                return this.Retrieve(x => x.InternationalPrefix);
            }
            set {
                this.Store(x => x.InternationalPrefix, value);
            }
        }

        [RegularExpression("^\\d+$", ErrorMessage = "Invalid phone number. Only digits accepted.")]
        public string PhoneNumber {
            get {
                return this.Retrieve(x => x.PhoneNumber);
            }
            set {
                this.Store(x => x.PhoneNumber, value);
            }
        }
    }

    [OrchardFeature("Laser.Orchard.Sms")]
    public class UserPwdRecoveryPartRecord : ContentPartRecord {
        public virtual string InternationalPrefix { get; set; }
        public virtual string PhoneNumber { get; set; }
    }
}