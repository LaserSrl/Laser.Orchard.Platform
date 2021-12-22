using Laser.Orchard.AdvancedSettings.Services;
using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Core.Common.Fields;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.PaymentGateway.Providers {
    [OrchardFeature("Laser.Orchard.CustomPaymentGateway")]
    public class BankTransferPosProvider : DefaultCustomPosProvider {
        private readonly IAdvancedSettingsService _advancedSettings;
        private readonly dynamic _shapeFactory;

        public BankTransferPosProvider(IWorkContextAccessor workContextAccessor,
            IAdvancedSettingsService advancedSettingsService,
            IShapeFactory shapeFactory) : base(workContextAccessor) {
            _advancedSettings = advancedSettingsService;
            _shapeFactory = shapeFactory;
        }

        public override string TechnicalName => "BankTransfer";

        public override string GetButtonShapeName() {
            return "BankTransfer";
        }

        public override string GetDisplayName() {
            return T("Bank Transfer").Text;
        }

        public override string GetInfoShapeName() {
            return "BankTransfer";
        }

        public override IEnumerable<dynamic> GetAdditionalFrontEndMetadataShapes(PaymentRecord payment) {
            if (!string.IsNullOrWhiteSpace(GetPosName(payment))) {
                var settingsCI = _advancedSettings.GetCachedSetting("Impostazioni Pagamenti");
                if (settingsCI != null) {
                    var iban = string.Empty;
                    var email = string.Empty;
                    var ibanField = settingsCI.ContentItem.Parts
                        .SelectMany(p => p.Fields)
                        .FirstOrDefault(f => f.Name
                            .Equals("IBAN", StringComparison.InvariantCultureIgnoreCase)) as TextField;
                    if (ibanField != null) {
                        iban = ibanField.Value;
                    }

                    var emailField = settingsCI.ContentItem.Parts
                        .SelectMany(p => p.Fields)
                        .FirstOrDefault(f => f.Name
                            .Equals("BankTransferEmail", StringComparison.InvariantCultureIgnoreCase)) as TextField;
                    if (emailField != null) {
                        email = emailField.Value;
                    }

                    var reason = payment.Reason;

                    return _shapeFactory.BankTransferOrderAdditionalFrontEndData(
                        Iban: iban,
                        Email: email,
                        Reason: reason
                    );
                }
            }

            return base.GetAdditionalFrontEndMetadataShapes(payment);
        }
    }
}