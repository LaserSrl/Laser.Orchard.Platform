using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Data;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway {
    public class PaymentEventHandler : IPaymentEventHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkflowManager _workflowManager;
        private readonly IRepository<PaymentRecord> _repository;

        public PaymentEventHandler(IOrchardServices orchardServices, IWorkflowManager workflowManager, IRepository<PaymentRecord> repository) {
            _orchardServices = orchardServices;
            _workflowManager = workflowManager;
            _repository = repository;
        }
        public void OnSuccess(int paymentId, int contentItemId) {
            var ci = _orchardServices.ContentManager.Get(contentItemId);
            var payment = _repository.Get(paymentId);
            _workflowManager.TriggerEvent("PaymentEnded", ci, () => new Dictionary<string, object> {
                {"Payment", payment}
            });
        }

        public void OnError(int paymentId, int contentItemId) {
            var ci = _orchardServices.ContentManager.Get(contentItemId);
            var payment = _repository.Get(paymentId);
            _workflowManager.TriggerEvent("PaymentEnded", ci, () => new Dictionary<string, object> {
                {"Payment", payment}
            });
        }
    }
}