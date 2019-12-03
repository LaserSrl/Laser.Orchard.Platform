using System.Collections.Generic;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.DisplayManagement;
using System.Web.Mvc;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IPosServiceIntegration : ICheckoutService {
        string GetOrderNumber(int orderId);
    }

    public class PosServiceIntegration : IPosServiceIntegration {
        private readonly IOrchardServices _orchardServices; 
        private readonly IEnumerable<IPosService> _posServices;
        private readonly dynamic _shapeFactory;
        private readonly IOrderService _orderService;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IPaymentService _paymentService;

        public PosServiceIntegration(
            IOrchardServices orchardServices, 
            IEnumerable<IPosService> posServices, 
            IShapeFactory shapeFactory,
            IOrderService orderService,
            ICurrencyProvider currencyProvider,
            IPaymentService paymentService) {
            _orchardServices = orchardServices;
            _posServices = posServices;
            _shapeFactory = shapeFactory;
            _orderService = orderService;
            _currencyProvider = currencyProvider;
            _paymentService = paymentService;
        }

        public string Name
        {
            get
            {
                return "Payment Gateway payments";
            }
        }

        public dynamic BuildCheckoutButtonShape(IEnumerable<dynamic> productShapes, IEnumerable<ShoppingCartQuantityProduct> productQuantities, IEnumerable<ShippingOption> shippingOptions, TaxAmount taxes, string country, string zipCode, IEnumerable<string> custom) {
            bool insertOrder = false;
            foreach(var opt in shippingOptions) {
                // check whether any shipping option is selected
                if(opt != null) {
                    insertOrder = true;
                }
            }
            if (!insertOrder) {
                // perhaps we need no shipping option
                // for example, if all products are digital
                insertOrder = !productQuantities.Any(pq => !pq.Product.IsDigital);
            }
            if (insertOrder) {
                return _shapeFactory.Pos();
            }
            else {
                return null;
            }
        }
        public string GetChargeAdminUrl(string transactionId) {
            string result = "";
            var payment = _paymentService.GetPaymentByGuid(transactionId);
            if(payment != null) {
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("Info", "Payment", new { area = "Laser.Orchard.PaymentGateway" });
                result = string.Format("{0}?paymentId={1}", url, payment.Id);
            }
            return result;
        }
        public string GetOrderNumber(int orderId) {
            return string.Format("KPO-{0}", orderId);
        }
    }
}