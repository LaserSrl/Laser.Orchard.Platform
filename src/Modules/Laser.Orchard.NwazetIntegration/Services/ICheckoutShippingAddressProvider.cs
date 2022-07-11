using Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface ICheckoutShippingAddressProvider : IDependency {

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
        /// Shapes meant to be injected in the shape for the form that
        /// where the user selects their shipping address.
        /// </summary>
        /// <param name="cvm">The current CheckoutViewModel under process.</param>
        /// <returns></returns>
        IEnumerable<AdditionalIndexShippingAddressViewModel>
            GetIndexShippingAddressShapes(CheckoutViewModel cvm);

        /// <summary>
        /// This will handle the values received when posting the addresses
        /// during the checkout process and extend the capabilities of the controller
        /// actions. If the provider is the actively selected one, this step may also 
        /// perform some initial validation and return the result from that. All other
        /// providers should return true, else hey would cause validation to fail for
        /// the checkout process.
        /// </summary>
        /// <param name="context"></param>
        bool ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm);

        /// <summary>
        /// Handle validation of the information input by the user
        /// </summary>
        /// <param name="cvm"></param>
        /// <returns></returns>
        bool ValidateAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm);

        /// <summary>
        /// Return the Id of the TerritoryPart for the Country of the Shipping destination
        /// according to this provider. Default to 0 if this provider cannot tell. This 
        /// method should on principle only be called for selected providers.
        /// </summary>
        /// <param name="cvm"></param>
        /// <returns></returns>
        int GetShippingCountryId(CheckoutViewModel cvm);

        /// <summary>
        /// Return the DisplayName of the TerritoryPart for the Country of the Shipping 
        /// destination according to this provider. Default to empty string if this 
        /// provider cannot tell. This method should on principle only be called for 
        /// selected providers.
        /// </summary>
        /// <param name="cvm"></param>
        /// <returns></returns>
        string GetShippingCountryName(CheckoutViewModel cvm);

        /// <summary>
        /// Return the Postal Code for the shipping destination accordin to this provider.
        /// Default ot null if this provider cannot tell. This method should on principle
        /// only be called for selected providers.
        /// </summary>
        /// <param name="cvm"></param>
        /// <returns></returns>
        string GetShippingPostalCode(CheckoutViewModel cvm);

        /// <summary>
        /// Perform necessary operations to reinflate shipping addresses that
        /// may be referenced from within the passed context. This method should on principle
        /// only be called for selected providers.
        /// </summary>
        /// <param name="context"></param>
        void ReinflateShippingAddress(ShippingAddressReinflationContext context);
    }
}