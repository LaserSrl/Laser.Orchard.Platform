using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.PaymentGateway.Providers {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public abstract class DefaultCustomPosProvider : ICustomPosProvider {
        public Localizer T { get; set; }

        public virtual string TechnicalName {
            get { return "DefaultCustomPos"; }
        }

        public DefaultCustomPosProvider() {
            T = NullLocalizer.Instance;
        }

        public virtual string GetButtonShapeName() {
            return "";
        }

        public virtual string GetDisplayName() {
            return T("Default Custom Pos").Text;
        }

        public virtual string GetInfoShapeName() {
            return "DefaultCustomPos";
        }
    }
}