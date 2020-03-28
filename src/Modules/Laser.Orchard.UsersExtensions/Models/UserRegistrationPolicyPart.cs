using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Laser.Orchard.Policy.ViewModels;
using Orchard.ContentManagement;

namespace Laser.Orchard.UsersExtensions.Models {
    public class UserRegistrationPolicyPart : ContentPart {
        public IEnumerable<PolicyAnswer> PolicyAnswers {
            get {
                return DeserializeRegistrationAnswers(this.Retrieve<string>("PolicyAnswers"));
            }
            set {
                this.Store("PolicyAnswers", SerializeRegistrationAnswers(value));
            }
        }
        private IEnumerable<PolicyAnswer> DeserializeRegistrationAnswers(string PolicyAnswers) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<IEnumerable<PolicyAnswer>>(PolicyAnswers ?? "{}");
        }

        private string SerializeRegistrationAnswers(IEnumerable<PolicyAnswer> PolicyAnswers) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(PolicyAnswers ?? new List<PolicyAnswer>());
        }

    }
}

