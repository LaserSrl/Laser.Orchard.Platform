using Orchard;
using Orchard.Localization;

namespace Laser.Orchard.PaymentGateway.Providers {
    public interface ICustomPosProvider : IDependency {
        Localizer T { get; set; }
        /// <summary>
        /// Technical name of the CustomPosProvider.
        /// This works like a unique identifier for the CustomPos type (e.g.: BankTransfer, CashOnDelivery, Satispay, ...).
        /// </summary>
        string TechnicalName { get; }
        /// <summary>
        /// This is the name displayed in back office and front end to the operator / users of the tenant.
        /// </summary>
        string GetDisplayName();
        /// <summary>
        /// Name of the partial to display into the info page, after the complete checkout procedure.
        /// </summary>
        string GetInfoShapeName();
        /// <summary>
        /// Name of the partial to display as the payment button during the checkout phase.
        /// </summary>
        string GetButtonShapeName();
        /// <summary>
        /// String representing the order status to set after the e-commerce payment procedure.
        /// </summary>
        /// <returns></returns>
        string GetOrderStatus();
    }
}
