using Laser.Orchard.UserReactions.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.Localization;

namespace Laser.Orchard.UserReactions.ViewModels {

    public class UserReactionsTypes {

        public UserReactionsTypes() {
            UserReactionsType = new List<UserReactionsTypeVM>();
        }

        public StyleFileNameProviders CssName { get; set; }
        public List<UserReactionsTypeVM> UserReactionsType { get; set; }
        public bool AllowMultipleChoices { get; set; }
    }

    public class UserReactionsTypeVM {
        public int Id { get; set; }
        [ValidateUserReactionsTypeVM]
        public string TypeName { get; set; }
        public int Priority { get; set; }
        public bool Activating { get; set; }
        public bool Delete { get; set; }
    }

    public class ValidateUserReactionsTypeVM : ValidationAttribute {
        public ValidateUserReactionsTypeVM() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            var model = (UserReactionsTypeVM)validationContext.ObjectInstance;


            if (model.Delete)
                return ValidationResult.Success; // it will be deleted, so no validation 
            else {
                if (validationContext.DisplayName == "TypeName") {
                    if (String.IsNullOrWhiteSpace(model.TypeName)) {
                        return new ValidationResult(T("Type Name field is required.").Text);
                    }
                }
            }
            return ValidationResult.Success;
        }
    }

}