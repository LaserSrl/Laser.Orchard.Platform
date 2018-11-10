using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Laser.Orchard.Policy.Models;
using System.Collections;
using Laser.Orchard.Policy.ViewModels;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.StartupConfig.Handlers;

namespace Laser.Orchard.Policy.Handlers {
    public class UserPolicyDumperHandler : IDumperHandler {
        private readonly IPolicyServices _policyServices;
        public UserPolicyDumperHandler(IPolicyServices policyServices) {
            _policyServices = policyServices;
        }
        public void StoreLikeDynamic(ContentItem item, string[] listProperty, object value) {
            var part = item.As<UserPolicyPart>();
            if (part != null) {
                if (listProperty.Length == 2) {
                    if (listProperty[0] == "UserPolicyPart") {
                        if (listProperty[1] == "UserPolicyAnswers") {
                            var newValues = new List<PolicyForUserViewModel>();
                            var arr = value as IEnumerable;
                            foreach (dynamic answer in arr) {
                                newValues.Add(new PolicyForUserViewModel {
                                    PolicyTextId = Convert.ToInt32(answer.Id),
                                    Accepted = answer.Accepted
                                });
                            }
                            _policyServices.PolicyForItemMassiveUpdate(newValues, item);
                        }
                    }
                }
            }
        }
    }
}