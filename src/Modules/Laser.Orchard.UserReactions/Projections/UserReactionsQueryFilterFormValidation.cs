using System;
using System.Linq;
using Orchard.Forms.Services;
using Orchard.Localization;
using Laser.Orchard.UserReactions.ViewModels;
using Laser.Orchard.UserReactions.Services;

namespace Laser.Orchard.UserReactions.Projections {

    public class UserReactionsQueryFilterFormValidation : FormHandler {
        public Localizer T { get; set; }
        private readonly IUserReactionsService _reactionsService;


        public UserReactionsQueryFilterFormValidation(IUserReactionsService reactionsService) {

            _reactionsService = reactionsService;
        }

        public override void Validating(ValidatingContext context) {
            
            
            if (context.FormName == UserReactionsQueryFilterForm.FormName) {

                //UserReactions validation
                ///////////////////////////////////////////////////////////
                string reaction = context.ValueProvider.GetValue("Reaction").AttemptedValue.ToString().ToLower();
                var userRT = new UserReactionsTypes();
                userRT.UserReactionsType = _reactionsService.GetTypesTable().Select(r => new UserReactionsTypeVM {
                    Id = r.Id,
                    TypeName = r.TypeName,
                }).ToList();

                int lenReaction = reaction.Length;
                var reactionsList = userRT.UserReactionsType;

                if ((reaction.Substring(0, 1) != "{" && reaction.Substring(lenReaction - 1, 1) != "}") && (reactionsList.FirstOrDefault(x => x.TypeName == reaction) == null))
                {                    
                    context.ModelState.AddModelError("Reaction", T("The field {0} should contain a valid input.", T("Reactions").Text).Text);
                }                

                if (!context.ModelState.IsValid) {
                    return;
                }
                ///////
                             
                var isRange = new[] { "Between", "NotBetween" }.Contains(context.ValueProvider.GetValue("Operator").AttemptedValue);
                var min = context.ValueProvider.GetValue("Min");
                var max = context.ValueProvider.GetValue("Max");
                var value = context.ValueProvider.GetValue("Value");

                // validating mandatory values
                if (isRange) {
                    if (min == null || String.IsNullOrWhiteSpace(min.AttemptedValue)) {
                        context.ModelState.AddModelError("Min", T("The field {0} is required.", T("Min").Text).Text);
                    }

                    if (max == null || String.IsNullOrWhiteSpace(max.AttemptedValue)) {
                        context.ModelState.AddModelError("Max", T("The field {0} is required.", T("Max").Text).Text);
                    }
                } else {
                    if (min == null || String.IsNullOrWhiteSpace(value.AttemptedValue)) {
                        context.ModelState.AddModelError("Value", T("The field {0} is required.", T("Value").Text).Text);
                    }
                }

                if (!context.ModelState.IsValid) {
                    return;
                }

                decimal output;

                if (isRange) {
                    if (!Decimal.TryParse(min.AttemptedValue, out output) && !IsToken(min.AttemptedValue)) {
                        context.ModelState.AddModelError("Min", T("The field {0} should contain a valid number", T("Min").Text).Text);
                    }

                    if (!Decimal.TryParse(max.AttemptedValue, out output) && !IsToken(max.AttemptedValue)) {
                        context.ModelState.AddModelError("Max", T("The field {0} should contain a valid number", T("Max").Text).Text);
                    }
                } else {
                    if (!Decimal.TryParse(value.AttemptedValue, out output) && !IsToken(value.AttemptedValue)) {
                        context.ModelState.AddModelError("Value", T("The field {0} should contain a valid number", T("Value").Text).Text);
                    }
                }
            }
        }

        private bool IsToken(string value) {
            return value.StartsWith("{") && value.EndsWith("}");
        }
    }
}