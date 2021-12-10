using Laser.Orchard.PaymentGateway.Models;
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
        /// Name of the partial to display into the info page, after the complete checkout procedure.
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        string GetInfoShapeName(PaymentRecord payment);
        /// <summary>
        /// Name of the partial to display as the payment button during the checkout phase.
        /// </summary>
        string GetButtonShapeName();
        /// <summary>
        /// Name of the pos based on the PaymentRecord.
        /// </summary>
        /// <param name="payment"></param>
        /// <returns>Returns an empty string if the pos isn't found between CustomPostProviders.</returns>
        string GetPosName(PaymentRecord payment);
    }
}
