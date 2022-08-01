using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointOrderPartDriver
        : ContentPartDriver<PickupPointOrderPart> {


        protected override string Prefix {
            get { return "PickupPointOrderPart"; }
        }

        protected override DriverResult Editor(
            PickupPointOrderPart part, dynamic shapeHelper) {

            return ContentShape("Parts_Order_PickupPoint_Edit", () => {
                var addressIsSame = false;
                if (part.IsOrderPickupPoint) {
                    // compare address in part with the one currently in the order
                    var addressPart = part.As<AddressOrderPart>();
                    // if anything is now different, flag it
                    if (addressPart != null) {
                        addressIsSame =
                            part.CountryName.Equals(addressPart.ShippingCountryName)
                            && part.CountryId == addressPart.CountryId
                            && part.ProvinceName.Equals(addressPart.ShippingProvinceName)
                            && part.ProvinceId == addressPart.ProvinceId
                            && part.CityName.Equals(addressPart.ShippingCityName)
                            && part.CityId == addressPart.CityId
                            ;
                    }
                    // street address and postal code is not stored in AddressOrderPart
                    if (addressIsSame) {
                        var orderPart = part.As<OrderPart>();
                        if (orderPart != null) {
                            var shippingAddress = orderPart.ShippingAddress;
                            // these strings may be null in both addresses
                            addressIsSame &= 
                                string.Equals(part.AddressLine1, shippingAddress.Address1)
                                && string.Equals(part.AddressLine2, shippingAddress.Address2)
                                && string.Equals(part.PostalCode, shippingAddress.PostalCode);
                        }
                    }
                }

                return shapeHelper.EditorTemplate(
                    Model: new PickupPointOrderViewModel {
                        Part = part,
                        AddressHasChanged = !addressIsSame
                    },
                    TemplateName: "Parts/Order.PickupPoint",
                    Prefix: Prefix
                    );
            });
        }
    }
}