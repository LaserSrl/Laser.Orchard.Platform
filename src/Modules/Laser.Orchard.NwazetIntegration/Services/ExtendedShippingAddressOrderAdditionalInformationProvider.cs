using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class ExtendedShippingAddressOrderAdditionalInformationProvider 
        : BaseOrderAdditionalInformationProvider {

        public override IEnumerable<XElement> PrepareAdditionalInformation(OrderContext context) {
            // Add to the order information that will allow us to rebuild contextual
            // information related to the ICheckoutShippingAddressProvider the customer 
            // selected during checkout.
            var exContext = (ExtendedOrderContext)context;
            if (exContext != null && exContext.CheckoutViewModel != null) {
                // The element should contain the Id of the selected provider. This will allow
                // to discriminate among active ones for the order (as long as the corresponding
                // features aren't deactivated).
                var providerXElement = new XElement(Constants.ShippingAddressProviderOrderXElementName)
                    .With(exContext.CheckoutViewModel)
                    .ToAttr(cvm => cvm.SelectedShippingAddressProviderId);

                yield return providerXElement;
            }
            yield break;
        }
    }
}