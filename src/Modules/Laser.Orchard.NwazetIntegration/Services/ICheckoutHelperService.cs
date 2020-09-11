using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Orchard;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface ICheckoutHelperService : IDependency {

        OrderPart CreateOrder(
            AddressesVM model,
            string paymentGuid,
            string countryName = null,
            string postalCode = null);
        /// <summary>
        /// Tests whether the user is allowed to checkout. If not, the method should
        /// provide the action the user should be redirected to.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="redirect"></param>
        /// <returns></returns>
        bool UserMayCheckout(IUser user, out ActionResult redirect);
        /// <summary>
        /// Tests whether the user is allowed to checkout.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool UserMayCheckout(IUser user);
    }
}
