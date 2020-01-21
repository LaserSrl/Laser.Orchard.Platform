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
        private readonly IRepository<PaymentRecord> _repository;
        private readonly IPaymentService _paymentService;

        public OrderPartDriver(
            IOrchardServices orchardServices,
            IRepository<PaymentRecord> repository,
            IPaymentService paymentService)
        {
            _orchardServices = orchardServices;
            _repository = repository;
            _paymentService = paymentService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(
            OrderPart part, string displayType, dynamic shapeHelper)
        {
            return null;
        }

        //GET
        protected override DriverResult Editor(OrderPart order, dynamic shapeHelper)
        {
            if (!_orchardServices.Authorizer.Authorize(OrderPermissions.ManageOrders, null, T("Cannot manage orders")))
                return null;

            var payment = _paymentService.GetPaymentByGuid(order.Charge.TransactionId);

            if(payment != null) {
                PaymentRecord result = _repository.Get(payment.Id);

                return ContentShape("Parts_Order_PaymentInfo",
                    () => {
                        var model = new PaymentInfoViewModel
                        {
                            PosName = result.PosName,
                            Reason = result.Reason,
                            Amount = result.Amount,
                            Currency = result.Currency,
                            UpdateDate= result.UpdateDate,
                            Success = result.Success,
                            Error = result.Error,
                            TransationId = result.TransactionId,
                        };
                        return shapeHelper.EditorTemplate(
                            TemplateName: "Parts/Order.PaymentInfo",
                            Model: model);
                    });
            }
            return null;
        }
    }
}