using Laser.Orchard.PaymentGateway;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Models;
using Laser.Orchard.NwazetIntegration.Services;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Workflows.Services;
using Laser.Orchard.NwazetIntegration.Models;

namespace Laser.Orchard.NwazetIntegration.Handlers {
    public class PaymentEventHandler : IPaymentEventHandler {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IShoppingCart _shoppingCart;
        private readonly INwazetCommunicationService _nwazetCommunicationService;
        private readonly IEnumerable<ICartLifeCycleEventHandler> _cartLifeCycleEventHandlers;
        private readonly IContentManager _contentManager;
        private readonly IWorkflowManager _workflowManager;
        private readonly IUpdateStatusService _updateStatusService;

        public PaymentEventHandler(
            IOrderService orderService, 
            IPaymentService paymentService, 
            IShoppingCart shoppingCart, 
            INwazetCommunicationService nwazetCommunicationService, 
            IEnumerable<ICartLifeCycleEventHandler> cartLifeCycleEventHandlers,
            IContentManager contentManager,
            IWorkflowManager workflowManager,
            IUpdateStatusService updateStatusService) {

            _orderService = orderService;
            _paymentService = paymentService;
            _shoppingCart = shoppingCart;
            _nwazetCommunicationService = nwazetCommunicationService;
            _cartLifeCycleEventHandlers = cartLifeCycleEventHandlers;
            _contentManager = contentManager;
            _workflowManager = workflowManager;
            _updateStatusService = updateStatusService;
        }
        public void OnError(int paymentId, int contentItemId) {
            var payment = _paymentService.GetPayment(paymentId);
            if (payment != null) {
                var order = _contentManager.Get<OrderPart>(payment.ContentItemId, VersionOptions.Latest);
                //order.Status = OrderPart.Cancelled;
                //_contentManager.Publish(order.ContentItem);
                _updateStatusService.UpdateOrderStatusChanged(order, Constants.PaymentFailed);
                
                _workflowManager.TriggerEvent(
                    "OrderError", 
                    order,
                    () => new Dictionary<string, object> {
                        {"Content", order},
                        {"Order", order},
                        {"CheckoutError", payment.Error}
                    });
                order.LogActivity(Constants.PaymentFailed, payment.Error);
                //order.LogActivity(OrderPart.Error, string.Format("Transaction failed (payment id: {0}).", payment.Id));
            }
        }

        public void OnSuccess(int paymentId, int contentItemId) {
            var payment = _paymentService.GetPayment(paymentId);
            if (payment != null) {
                var order = _contentManager.Get<OrderPart>(payment.ContentItemId, VersionOptions.Latest); // _orderService.Get(payment.ContentItemId);
                // aggiorna l'odine in base al pagamento effettuato
                //order.Status = OrderPart.Pending;
                order.Status = Constants.PaymentSucceeded;
                order.AmountPaid = payment.Amount;
                order.CurrencyCode = payment.Currency;
                // update charge
                order.UpdateCharge(
                    new PaymentGatewayCharge("Payment Gateway", payment.Guid));
                order.LogActivity(OrderPart.Event, string.Format("Payed on POS {0}.", payment.PosName));
                // svuota il carrello
                foreach (var handler in _cartLifeCycleEventHandlers) {
                    handler.Finalized();
                }
                _shoppingCart.Clear();
                // raise order and payment events
                _contentManager.Publish(order.ContentItem);
                TriggerEvents(order);
            }
        }
        
        private void TriggerEvents(OrderPart order) {
            TriggerEvent(order, "NewOrder");
            TriggerEvent(order, "NewPayment");
        }
        private void TriggerEvent(OrderPart order, string eventName) {
            _workflowManager.TriggerEvent(
                   eventName,
                   order,
                   () => new Dictionary<string, object> {
                        {"Content", order},
                        {"Order", order}
                   });
        }
    }
}