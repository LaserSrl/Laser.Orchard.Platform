using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Laser.Orchard.Policy.ViewModels {
    public class PoliciesApiModel {
        public string Language { get; set; }
        public SimplePoliciesForUserViewModel PoliciesForUser { get; set; }
    }

    public class PolicyAnswer {
        public int PolicyTextId { get; set; }
        public bool Accepted { get; set; }
    }


    public class PoliciesForUserViewModel {
        public PoliciesForUserViewModel() {
            Policies = new List<PolicyForUserViewModel>();
            EditMode = false;
        }

        public IList<PolicyForUserViewModel> Policies { get; set; }
        public bool EditMode { get; set; }
    }

    public class SimplePoliciesForUserViewModel {
        public SimplePoliciesForUserViewModel() {
            Policies = new List<SimplePolicyForUserViewModel>();
        }
        public IList<SimplePolicyForUserViewModel> Policies { get; set; }
    }
    public class PolicyForUserViewModel : SimplePolicyForUserViewModel {
        [ScriptIgnore]
        public Models.PolicyTextInfoPart PolicyText { get; set; }
    }

    public class SimplePolicyForUserViewModel {
        public int AnswerId { get; set; }
        public int PolicyTextId { get; set; }
        public DateTime AnswerDate { get; set; }
        public bool OldAccepted { get; set; }
        public bool Accepted { get; set; }
        public int? UserId { get; set; }
    }

    public class PolicyHistoryViewModel {
        public int PolicyId { get; set; }
        public string PolicyTitle { get; set; }
        public PolicyTypeOptions PolicyType { get; set; }
        public bool Accepted { get; set; }
        public DateTime AnswerDate { get; set; }
        public DateTime? EndValidity { get; set; }
        public string UserName { get; set; }
    }
}