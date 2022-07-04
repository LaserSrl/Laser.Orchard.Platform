using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface ICheckoutExtensionProvider : IDependency {
        /// <summary>
        /// Shapes meant to be injected in the shape for the form that
        /// starts the checkout process.
        /// </summary>
        /// <returns></returns>
        IEnumerable<dynamic> AdditionalCheckoutStartShapes();

        /// <summary>
        /// This will validate the values received when posting to start
        /// the checkout process.
        /// </summary>
        /// <param name="context"></param>
        void ProcessAdditionalCheckoutStartInformation(
            CheckoutExtensionContext context);

        /// <summary>
        /// Shapes meant to be injected in the shape for the form that
        /// where the user selects their shipping address.
        /// </summary>
        /// <param name="cvm">The current CheckoutViewModel under process.</param>
        /// <returns></returns>
        IEnumerable<AdditionalIndexShippingAddressViewModel> 
            AdditionalIndexShippingAddressShapes(CheckoutViewModel cvm);

        /// <summary>
        /// Used to filter and only select providers that should actually do stuff
        /// during the index steps of checkout. This is used in the POST actions, 
        /// when the User has already selected some stuff, because all providers
        /// should contribute in giving the user options in the GET actions.
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        bool IsSelectedProviderForIndex(string providerId);

        /// <summary>
        /// Handle validation of the information input by the user
        /// </summary>
        /// <param name="cvm"></param>
        /// <returns></returns>
        bool ValidateAdditionalIndexShippingAddressInformation(
            CheckoutViewModel cvm);

        /// <summary>
        /// This will handle the values received when posting the addresses
        /// during the checkout process and extend the capabilities of the controller
        /// actions.
        /// </summary>
        /// <param name="context"></param>
        void ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm);
    }
}
