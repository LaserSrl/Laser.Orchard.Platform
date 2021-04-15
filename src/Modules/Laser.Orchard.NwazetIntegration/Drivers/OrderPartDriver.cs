using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Laser.Orchard.PaymentGateway.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Orchard;
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

        public OrderPartDriver(
            IOrchardServices orchardServices,
            IPaymentService paymentService)
        {
            _orchardServices = orchardServices;
            _paymentService = paymentService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

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
                        TransationId = payment.TransactionId,
                    };
                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts/Order.PaymentInfo",
                        Model: model);
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