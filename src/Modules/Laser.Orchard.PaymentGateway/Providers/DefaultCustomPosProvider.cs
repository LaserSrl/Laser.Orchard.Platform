using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.PaymentGateway.Providers {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public abstract class DefaultCustomPosProvider : ICustomPosProvider {
        public Localizer T { get; set; }
        protected readonly IWorkContextAccessor _workContextAccessor;

        public virtual string TechnicalName {
            get { return "DefaultCustomPos"; }
        }

        public DefaultCustomPosProvider(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
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

        public virtual string GetPosServiceName(string posServiceName) {
            if (posServiceName.StartsWith("CustomPos_")) {
                posServiceName = posServiceName.Substring("CustomPos_".Length);
            }

            var customPosSiteSettings = _workContextAccessor.GetContext().CurrentSite.As<CustomPosSiteSettingsPart>();
            var posFound = customPosSiteSettings.CustomPos
                .Any(cps => cps.Name.Equals(posServiceName));
            if (posFound) {
                return "Custom Pos";
            }
            return string.Empty;
        }

        public virtual string GetInfoShapeName(PaymentRecord payment) {
            var customPosName = payment.PosName;
            if (customPosName.StartsWith("CustomPos_")) {
                customPosName = customPosName.Substring("CustomPos_".Length);
            }

            var customPosSiteSettings = _workContextAccessor.GetContext().CurrentSite.As<CustomPosSiteSettingsPart>();
            var currentCustomPos = customPosSiteSettings.CustomPos
                .FirstOrDefault(cps => cps.Name.Equals(customPosName));
            if (currentCustomPos != null && currentCustomPos.ProviderName.Equals(TechnicalName, StringComparison.InvariantCultureIgnoreCase)) {
                return GetInfoShapeName();
            }

            return string.Empty;
        }

        public virtual string GetPosName(PaymentRecord payment) {
            var customPosName = payment.PosName;
            if (customPosName.StartsWith("CustomPos_")) {
                customPosName = customPosName.Substring("CustomPos_".Length);
            }

            var customPosSiteSettings = _workContextAccessor.GetContext().CurrentSite.As<CustomPosSiteSettingsPart>();
            var currentCustomPos = customPosSiteSettings.CustomPos
                .FirstOrDefault(cps => cps.Name.Equals(customPosName));
            if (currentCustomPos != null && currentCustomPos.ProviderName.Equals(TechnicalName, StringComparison.InvariantCultureIgnoreCase)) {
                return customPosName;
            }

            return string.Empty;
        }
        
        public virtual IEnumerable<dynamic> GetAdditionalFrontEndMetadataShapes(PaymentRecord payment) {
            return Enumerable.Empty<dynamic>();
        }
    }
}