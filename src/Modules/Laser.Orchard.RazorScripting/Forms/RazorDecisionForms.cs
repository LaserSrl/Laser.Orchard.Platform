using System;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Laser.Orchard.RazorScripting.Forms {

    public class RazorDecisionForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public RazorDecisionForms(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
              shape => Shape.Form(
                Id: "RazorActionDecision",
                _RazorName: Shape.Textbox(
                  Id: "razorname", Name: "RazorName",
                  Title: T("Razor name."),
                  Description: T("A name for the razor that explain what it does."),
                  Classes: new[] { "text medium" }),
                _Message: Shape.Textbox(
                  Id: "outcomes", Name: "Outcomes",
                  Title: T("Possible Outcomes."),
                  Description: T("A comma-separated list of possible outcomes."),
                  Classes: new[] { "text medium" }),
                _RazorScript: Shape.TextArea(
                  Id: "RazorScript", Name: "RazorScript",
                  Title: T("RazorScript"),
                  Description: T("The script to run every time the Razor Decision Activity is invoked. You can use Model.ContentItem, Model.OrchardServices, Model.T(), Model.Tokens. If the script renders a string, that string has threated as outcome."),
                  Classes: new[] { "tokenized" }
                  )
                );

            context.Form("RazorActivityActionDecision", form);
        }
    }

    public class DecisionFormsValidator : IFormEventHandler {
        public Localizer T { get; set; }

        public void Building(BuildingContext context) {
        }

        public void Built(BuildingContext context) {
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName == "RazorActivityActionDecision") {
                if (context.ValueProvider.GetValue("RazorScript").AttemptedValue == string.Empty) {
                    context.ModelState.AddModelError("RazorScript", T("You must provide a Script").Text);
                }
            }
        }

        public void Validated(ValidatingContext context) {
        }
    }
}