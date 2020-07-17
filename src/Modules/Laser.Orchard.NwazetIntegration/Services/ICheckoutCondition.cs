using Orchard;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface ICheckoutCondition : IDependency {
        /// <summary>
        /// used to order the conditions. Highest priority goes first. 
        /// Conditions with lower priority may not be evaluated if previous ones
        /// tell to prevent checkout.
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Tests whether the user is allowed to checkout.
        /// </summary>
        /// <param name="user">The user to be tested (usually the current user)</param>
        /// <param name="redirect">An action to redirect to in case the user may
        /// not checkout to control where to redirect them.</param>
        /// <returns></returns>
        bool UserMayCheckout(IUser user, out ActionResult redirect);
        /// <summary>
        /// Tests whether the user is allowed to checkout.
        /// </summary>
        /// <param name="user">The user to be tested (usually the current user)</param>
        /// <returns></returns>
        bool UserMayCheckout(IUser user);
    }
}
