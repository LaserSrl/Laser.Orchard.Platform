using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Laser.Orchard.PaymentGateway.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class OrderPartDriver : ContentPartDriver<OrderPart> {

        private readonly IOrchardServices _orchardServices;
        private readonly IPaymentService _paymentService;
        private readonly IWorkContextAccessor _workContextAccessor;

        private string PaymentInfoPrefix => "OrderPaymentInfo";

        public OrderPartDriver(
            IOrchardServices orchardServices,
            IPaymentService paymentService,
            IWorkContextAccessor workContextAccessor)
        {
            _orchardServices = orchardServices;
            _paymentService = paymentService;
            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(OrderPart order, IUpdateModel updater, dynamic shapeHelper) {
            if (!_orchardServices.Authorizer.Authorize(OrderPermissions.ManageOrders, null, T("Cannot manage orders")))
                return null;

            if (updater != null) {
                var viewModel = new PaymentInfoViewModel();
                updater.TryUpdateModel(viewModel, PaymentInfoPrefix, null, null);

                if (viewModel.EditTransactionId) {
                    // I need to edit payment transaction id if changed.
                    var transactionId = viewModel.TransactionId;
                    if (transactionId != null) {
                        var payment = _paymentService.GetPaymentByGuid(order.Charge?.TransactionId);
                        if (!payment.TransactionId.Equals(transactionId, StringComparison.InvariantCultureIgnoreCase)) {
                            var eventText = T("Transaction id changed from {0} to {1}", payment.TransactionId, transactionId);
                            payment.TransactionId = transactionId;
                            order.LogActivity(OrderPart.Event, eventText.Text, _orchardServices.WorkContext.CurrentUser?.UserName ?? "System");
                        }
                    }
                }
            }

            return Editor(order, shapeHelper);
        }

        //GET
        protected override DriverResult Editor(OrderPart order, dynamic shapeHelper)
        {
            if (!_orchardServices.Authorizer.Authorize(OrderPermissions.ManageOrders, null, T("Cannot manage orders")))
                return null;

            var shapes = new List<DriverResult>();

            // PayentInfo
            PaymentRecord payment = _paymentService.GetPaymentByGuid(order.Charge?.TransactionId);

            shapes.Add( ContentShape("Parts_Order_PaymentInfo",
                () => {
                    if (payment == null) {
                        return null;
                    }

                    var model = new PaymentInfoViewModel {
                        PosName = payment.PosName,
                        Reason = payment.Reason,
                        Amount = payment.Amount,
                        Currency = payment.Currency,
                        UpdateDate = payment.UpdateDate,
                        Success = payment.Success,
                        Error = payment.Error,
                        TransactionId = payment.TransactionId,
                        EditTransactionId = false
                    };
                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts/Order.PaymentInfo",
                        Model: model,
                        Prefix: PaymentInfoPrefix);
                }));

            // Checkout policies
            var policyXElements = order.AdditionalElements
                .Where(el => CheckoutPolicySettingsPart.OrderXElementName
                    .Equals(el.Name.ToString(), StringComparison.InvariantCultureIgnoreCase));
            if (policyXElements.Any()) {
                var vm = new CheckoutPoliciesOrderViewModel();
                vm.Policies.AddRange(policyXElements.Select(xel => {
                    var partXel = xel.Element("PolicyTextInfoPart");
                    var pvm = new CheckoutPolicyOrderViewModel();
                    pvm.Accepted = bool.Parse(xel.Attribute("Accepted").Value);
                    pvm.Mandatory = bool.Parse(partXel.Attribute("UserHaveToAccept").Value);
                    pvm.AnswerDateUTC = DateTime.Parse(xel.Attribute("AnswerDate").Value).ToUniversalTime();
                    pvm.PolicyTextInfoPartId = int.Parse(xel.Attribute("PolicyTextId").Value);
                    pvm.PolicyTextInfoPartVersionNumber = int.Parse(xel.Attribute("VersionNumber").Value);
                    return pvm;
                }));
                shapes.Add(ContentShape("Parts_Order_CheckoutPolicies",
                    () => {
                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts/Order.CheckoutPolicies",
                        Model: vm);
                    }));
            }

            return Combined(shapes.ToArray());
        }
    }
}