using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Orchard.DisplayManagement;

namespace Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders {
    public abstract class BaseCheckoutShippingAddressProvider
        : ICheckoutShippingAddressProvider {

        protected readonly dynamic _shapeFactory;
        
        public virtual string GetShippingAddressProviderId()
        {
            return "default";
        }

        protected BaseCheckoutShippingAddressProvider(
            IShapeFactory shapeFactory) {

            _shapeFactory = shapeFactory;
        }

        public virtual IEnumerable<AdditionalIndexShippingAddressViewModel> 
            GetIndexShippingAddressShapes(CheckoutViewModel cvm) {
            yield break;
        }

        public virtual IEnumerable<AdditionalCheckoutShippingAddressSummaryViewModel>
            GetSummaryShippingAddressShapes(CheckoutViewModel cvm) {
            yield break;
        }


        public virtual int GetShippingCountryId(CheckoutViewModel cvm) {
            return 0;
        }

        public virtual int GetShippingProvinceId(CheckoutViewModel cvm) {
            return 0;
        }

        public virtual int GetShippingCityId(CheckoutViewModel cvm) {
            return 0;
        }

        public virtual string GetShippingCountryName(CheckoutViewModel cvm) {
            return string.Empty;
        }

        public virtual string GetShippingPostalCode(CheckoutViewModel cvm) {
            return string.Empty;
        }

        public virtual bool IsSelectedProviderForIndex(string providerId) {
            return false;
        }

        public virtual bool ProcessAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm) {
            return true;
        }

        public virtual bool ValidateAdditionalIndexShippingAddressInformation(
            CheckoutExtensionContext context, CheckoutViewModel cvm) {
            return true;
        }

        public virtual void ReinflateShippingAddress(ShippingAddressReinflationContext context) { }

        public virtual void ReinflateViewModel(CheckoutViewModel viewModel) { }

        public virtual IEnumerable<dynamic> GetOrderShippingAddressShapes(OrderPart orderPart) {
            yield return _shapeFactory.BaseOrderShippingAddress(
                OrderPart: orderPart
                );
        }
    }
}