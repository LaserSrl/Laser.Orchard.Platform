using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class CheckoutPoliciesOrderViewModel {

        public CheckoutPoliciesOrderViewModel() {
            Policies = new List<CheckoutPolicyOrderViewModel>();
        }

        public List<CheckoutPolicyOrderViewModel> Policies { get; set; }
    }

    public class CheckoutPolicyOrderViewModel {
        public bool Accepted { get; set; }
        public bool Mandatory { get; set; }
        public DateTime AnswerDateUTC { get; set; }

        public int PolicyTextInfoPartId { get; set; }
        public int PolicyTextInfoPartVersionNumber { get; set; }

    }
}