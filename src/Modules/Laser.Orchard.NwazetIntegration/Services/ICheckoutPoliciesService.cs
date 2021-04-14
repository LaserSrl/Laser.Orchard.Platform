using Laser.Orchard.Policy.ViewModels;
using Orchard;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface ICheckoutPoliciesService : IDependency {
        bool UserHasAllAcceptedPolicies(IUser user = null, string culture = null);
        IEnumerable<PolicyForUserViewModel> CheckoutPoliciesForUser(IUser user = null, string culture = null);
    }
}
