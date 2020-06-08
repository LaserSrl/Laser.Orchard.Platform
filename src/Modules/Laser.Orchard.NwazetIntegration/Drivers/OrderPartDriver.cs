using Laser.Orchard.NwazetIntegration.ViewModels;
using Laser.Orchard.PaymentGateway.Models;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;

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

            PaymentRecord payment = _paymentService.GetPaymentByGuid(order.Charge?.TransactionId);

            return ContentShape("Parts_Order_PaymentInfo",
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
                });
        }
    }
}