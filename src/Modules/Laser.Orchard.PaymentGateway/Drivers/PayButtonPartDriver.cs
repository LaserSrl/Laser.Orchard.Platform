using Laser.Orchard.PaymentGateway.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Laser.Orchard.PaymentGateway.Drivers {
    public class PayButtonPartDriver : ContentPartDriver<PayButtonPart> {
        public Localizer L;
        private readonly ITokenizer _tokenizer;
        private readonly IPaymentService _paymentService;

        public PayButtonPartDriver(ITokenizer tokenizer, IPaymentService paymentService) {
            L = NullLocalizer.Instance;
            _tokenizer = tokenizer;
            _paymentService = paymentService;
        }
        protected override string Prefix {
            get {
                return "Laser.Orchard.PaymentGateway";
            }
        }
        protected override DriverResult Display(PayButtonPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "Detail") {
                var partSettings = part.Settings.GetModel<PayButtonPartSettings>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                dynamic ci = part.ContentItem;
                var payment = new PaymentRecord();
                payment.ContentItemId = part.Id;
                payment.Currency = partSettings.DefaultCurrency;
                if (string.IsNullOrWhiteSpace(partSettings.CurrencyField) == false) {
                    payment.Currency = _tokenizer.Replace(partSettings.CurrencyField, tokens);
                }
                payment.Amount = Convert.ToDecimal(_tokenizer.Replace(partSettings.AmountField, tokens), CultureInfo.InvariantCulture);
                if (part.ContentItem.Parts.SingleOrDefault(x => x.PartDefinition.Name == "TitlePart") != null) {
                    payment.Reason = ci.TitlePart.Title;
                }
                var nonce = _paymentService.CreatePaymentNonce(payment);
                return ContentShape("Parts_PayButton",
                    () => shapeHelper.Parts_PayButton(Nonce: nonce));
            }
            else {
                return null;
            }
        }
        //GET
        protected override DriverResult Editor(PayButtonPart part, dynamic shapeHelper) {
            var viewModel = part;
            return ContentShape("Parts_PayButton_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/PayButtonEdit",
                    Model: viewModel,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(PayButtonPart part, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, shapeHelper);
        }
    }
}