using System;
using System.ComponentModel.DataAnnotations;
using Orchard.Localization;

namespace Laser.Orchard.ButtonToWorkflows.ViewModels.ValidationProviders {
    public class ValidateDynamicButtonData : ValidationAttribute {

        public Localizer T { get; set; }

        public ValidateDynamicButtonData() {
            T = NullLocalizer.Instance;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            DynamicButtonToWorkflowsEdit dynButtonEditVM = (DynamicButtonToWorkflowsEdit)value;

            if (dynButtonEditVM.Delete)
                return ValidationResult.Success;
            else {
                if (String.IsNullOrWhiteSpace(dynButtonEditVM.ButtonName))
                    return new ValidationResult(T("The Button Name field is required.").Text);
                else
                    return ValidationResult.Success;
            }
        }
    }
}