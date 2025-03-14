using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Mobile.Models {
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

    public class UserPwdRecoveryPartRecord : ContentPartRecord {
        public virtual string InternationalPrefix { get; set; }
        public virtual string PhoneNumber { get; set; }
    }
}