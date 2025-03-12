using Laser.Orchard.Mobile.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Mobile.Services {

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsValidationService : ValidationAttribute {

        public Localizer T { get; set; }

        public SmsValidationService() {
            T = NullLocalizer.Instance;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var model = (SmsGatewayPart)validationContext.ObjectInstance;

            if (model.SendToTestNumber && model.ContentItem.ContentType == "CommunicationAdvertising") {
                // PrefixForTest
                if (validationContext.DisplayName == "PrefixForTest") {
                    if (String.IsNullOrWhiteSpace(model.PrefixForTest)) {
                        return new ValidationResult(T("PrefixForTest field is required.").Text);
                    }
                }
                // NumberForTest
                else if (validationContext.DisplayName == "NumberForTest") {
                    if (String.IsNullOrWhiteSpace(model.NumberForTest)) {
                        return new ValidationResult(T("NumberForTest field is required.").Text);
                    } else if (model.NumberForTest.Trim().Length < 10) {
                        return new ValidationResult(T("NumberForTest field must be minimum 22 characters length.").Text);
                    }
                }
            }

            return ValidationResult.Success;
        }

    }
}