using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services.Pos;
using Laser.Orchard.PaymentGateway;
using Nwazet.Commerce.Events;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class PaymentEventHandler : IPaymentEventHandler {
        private readonly IPaymentService _paymentService;
        private readonly IShoppingCart _shoppingCart;
        private readonly IEnumerable<ICartLifeCycleEventHandler> _cartLifeCycleEventHandlers;
        private readonly IContentManager _contentManager;
        private readonly IUpdateStatusService _updateStatusService;
        private readonly IEnumerable<IPosIntegrationService> _posIntegrationServices;
        private readonly IEnumerable<IOrderEventHandler> _orderEventHandlers;

        public PaymentEventHandler( 
            IPaymentService paymentService, 
            IShoppingCart shoppingCart, 
            IEnumerable<ICartLifeCycleEventHandler> cartLifeCycleEventHandlers,
            IContentManager contentManager,
            IUpdateStatusService updateStatusService,
            IEnumerable<IPosIntegrationService> posServices,
            IEnumerable<IOrderEventHandler> orderEventHandlers) {

            _paymentService = paymentService;
            _shoppingCart = shoppingCart;
            _cartLifeCycleEventHandlers = cartLifeCycleEventHandlers;
            _contentManager = contentManager;
            _updateStatusService = updateStatusService;
            _posIntegrationServices = posServices;
            _orderEventHandlers = orderEventHandlers;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger;

        public void OnError(int paymentId, int contentItemId) {
            var payment = _paymentService.GetPayment(paymentId);
            if (payment != null) {
                var order = _contentManager.Get<OrderPart>(payment.ContentItemId, VersionOptions.Latest);

                _updateStatusService.UpdateOrderStatusChanged(order, Constants.PaymentFailed);

                _orderEventHandlers.Invoke(
                    h => h.OnOrderError(order, 
                        new Dictionary<string, string> { { "CheckoutError", payment.Error } }), 
                    Logger);
                order.LogActivity(Constants.PaymentFailed, payment.Error, "System");
            }
        }

        public void OnSuccess(int paymentId, int contentItemId) {
            var payment = _paymentService.GetPayment(paymentId);
            if (payment != null) {
                var order = _contentManager.Get<OrderPart>(payment.ContentItemId, VersionOptions.Latest);
                // aggiorna l'ordine in base al pagamento effettuato
                // I need to get the order status from the right pos service.
                var pos = _posIntegrationServices
                    .FirstOrDefault(ps => ps.CheckPos(payment));
                if (pos != null) {
                    order.Status = pos.GetPaymentSuccessOrderStatus().StatusName;
                } else {
                    order.Status = Constants.PaymentSucceeded;
                }
                order.AmountPaid = payment.Amount;
                order.CurrencyCode = payment.Currency;
                // update charge
                order.UpdateCharge(
                    new PaymentGatewayCharge("Payment Gateway", payment.Guid));
                order.LogActivity(OrderPart.Event, string.Format("Payed on POS {0}.", payment.PosName), "System");
                // svuota il carrello
                var cartContext = new CartFinalizedContext() { Order = order };
                _cartLifeCycleEventHandlers.Invoke(h => h.Finalized(cartContext), Logger);
                _shoppingCart.ClearAll();
                // raise order and payment events
                _contentManager.Publish(order.ContentItem);
                _orderEventHandlers.Invoke(h => h.OnNewOrder(order), Logger);
                _orderEventHandlers.Invoke(h => h.OnNewPayment(order), Logger);
            }
        }
        
    }
}