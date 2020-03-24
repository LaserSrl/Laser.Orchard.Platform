using Newtonsoft.Json;
using Nwazet.Commerce.Models;
using Orchard.Data;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class GTMMeasuringPurchaseService : IGTMMeasuringPurchaseService {
        private readonly IRepository<OrderVatRecord> _vatOrderRepository;
        public GTMMeasuringPurchaseService(
            IRepository<OrderVatRecord> vatOrderRepository) {
            _vatOrderRepository = vatOrderRepository;
        }

        public decimal GetVatDue(OrderPart orderPart) {
            var info = _vatOrderRepository
                .Fetch(ovr => ovr.OrderPartRecord == orderPart.Record)
                .FirstOrDefault();

            if (info == null) {
                return 0;
            }
            var data = DeserializeInformation(info.Information);
            var vatDue = orderPart.Items
                .Sum(checkoutItem =>
                    data.ContainsKey(checkoutItem.ProductId)
                        ? TaxDue(data[checkoutItem.ProductId]) * checkoutItem.Quantity
                        : 0m
                );

            //add shipping
            var shippingTax = 0.0m;
            if (orderPart.ShippingOption != null) {
                if (data.ContainsKey(orderPart.ShippingOption.ShippingMethodId)) {
                    var rateAndPrice = data[orderPart.ShippingOption.ShippingMethodId];
                    shippingTax += TaxDue(rateAndPrice);
                }
            }
            // total VAT = VAT applied to products +VAT applied to shipping
            return vatDue + shippingTax;
        }

        private static Dictionary<int, RateAndPrice> DeserializeInformation(string info) {
            return JsonConvert.DeserializeObject<Dictionary<int, RateAndPrice>>(info);
        }

        private static decimal TaxDue(RateAndPrice rap) {
            return rap.PriceBeforeTax * rap.Rate;
        }

        struct RateAndPrice {
            public decimal Rate { get; set; }
            public decimal PriceBeforeTax { get; set; }
        }
    }
}