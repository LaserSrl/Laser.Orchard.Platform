using System;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.RazorCodeExecution.Forms {
    [OrchardFeature("Laser.Orchard.StartupConfig.WorkflowExtensions")]
    public class RazorExecuteForm : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public RazorExecuteForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
              shape => Shape.Form(
                Id: "RazorExecuteActivity",
                _RazorExecuteActivity_Outcomes: Shape.Textbox(
                  Id: "RazorExecuteActivity_Outcomes", Name: "RazorExecuteActivity_Outcomes",
                  Title: T("Possible Outcomes."),
                  Description: T("A comma-separated list of possible outcomes."),
                  Classes: new[] { "text medium" }),
                _RazorExecuteActivity_RazorView: Shape.Textbox(
                  Id: "RazorExecuteActivity_RazorView", Name: "RazorExecuteActivity_RazorView",
                  Title: T("Razor View"),
                  Description: T("The razor view to run every time the Decision Activity is invoked. You can use Model.ContentItem, Model.T, Model.OrchardServices, Model.Tokens, Model.Tokens[\"Workflow\"] (to get WorkflowContext and call HasState, GetState, SetState). The razor output (e.g. 'Success', 'Error' or 'Empty') should be used to define the 'texthint' outcome of the activity."),
                  Classes: new[] { "text large tokenized" }
                  )
                );

            context.Form("RazorExecuteForm", form);
        }

    }

    [OrchardFeature("Laser.Orchard.StartupConfig.WorkflowExtensions")]
    public class RazorExecuteFormValidator : IFormEventHandler {
        public Localizer T { get; set; }

        public void Building(BuildingContext context) {
        }

        public void Built(BuildingContext context) {
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName == "RazorExecuteForm") {
                if (context.ValueProvider.GetValue("RazorExecuteActivity_RazorView").AttemptedValue == string.Empty) {
                    context.ModelState.AddModelError("RazorExecuteActivity_RazorView", T("You must provide a Razor View").Text);
                }
            }
        }

        public void Validated(ValidatingContext context) {

        }
    }

}