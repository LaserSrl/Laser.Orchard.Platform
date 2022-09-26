using Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
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
        /// Shapes meant to be injected in the shape for the form 
        /// where the user selects their shipping address.
        /// </summary>
        /// <param name="cvm">The current CheckoutViewModel under process.</param>
        /// <returns></returns>
        IEnumerable<AdditionalIndexShippingAddressViewModel>
            GetIndexShippingAddressShapes(CheckoutViewModel cvm);

        /// <summary>
        /// Shapes meant to be injected as small summary views across the checkout
        /// process to remind the user of the selections they have made. Generally,
        /// an implementation should only return somthing for the actively selected
        /// provider, and only if the user has correctly selected a valid address.
        /// </summary>
        /// <param name="cvm"></param>
        /// <returns></returns>
        IEnumerable<AdditionalCheckoutShippingAddressSummaryViewModel>
            GetSummaryShippingAddressShapes(CheckoutViewModel cvm);

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
        /// Return the Id of the TerritoryPart for the Country of the Shipping destination
        /// according to this provider. Default to 0 if this provider cannot tell. This 
        /// method should on principle only be called for selected providers.
        /// </summary>
        /// <param name="cvm"></param>
        /// <returns></returns>
        int GetShippingProvinceId(CheckoutViewModel cvm);

        /// <summary>
        /// Return the Id of the TerritoryPart for the Country of the Shipping destination
        /// according to this provider. Default to 0 if this provider cannot tell. This 
        /// method should on principle only be called for selected providers.
        /// </summary>
        /// <param name="cvm"></param>
        /// <returns></returns>
        int GetShippingCityId(CheckoutViewModel cvm);

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
        /// only be called for selected providers. In implementations of this method, selected
        /// providers should take care of assigning values to the ShippingAddress property
        /// of the CheckoutViewmodel that will be used at the end of the checkout process to
        /// create a new Order.
        /// </summary>
        /// <param name="context"></param>
        void ReinflateShippingAddress(ShippingAddressReinflationContext context);

        /// <summary>
        /// Perform some operations to reinflate the part of the checkout view model
        /// thhe implementation of this interface is responsible for.
        /// </summary>
        /// <param name="viewModel"></param>
        void ReinflateViewModel(CheckoutViewModel viewModel);

        /// <summary>
        /// This method returns shapes to be used to display Shipping addresses in
        /// order confirmations.
        /// </summary>
        /// <param name="orderPart"></param>
        /// <returns></returns>
        IEnumerable<dynamic> GetOrderShippingAddressShapes(OrderPart orderPart);

        /// <summary>
        /// This method returns the shipping address provider id used in the checkout controller.
        /// </summary>
        /// <returns></returns>
        string GetShippingAddressProviderId();
    }
}