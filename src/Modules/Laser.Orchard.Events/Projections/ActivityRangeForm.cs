using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Events;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.UI.Resources;
using System;

namespace Laser.Orchard.Events.Projections
{
    public interface IFormProvider : IEventHandler
    {
        void Describe(dynamic context);
    }

    public class ActivityRangeForm : IFormProvider
    {
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }
        private readonly Work<IResourceManager> _resourceManager;

        public ActivityRangeForm(IShapeFactory shapeFactory, Work<IResourceManager> resourceManager)
        {
            _resourceManager = resourceManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context)
        {
            Func<IShapeFactory, dynamic> form =
                shape =>
                {
                    var f = Shape.Form(
                        Id: "ActivityRangeForm",
                        _DateFrom: Shape.TextBox(
                            Id: "DateFrom", Name: "DateFrom",
                            Title: T("Activities from date"),
                            Description: T("Retrieve activities beginning in or after this date"),
                            Classes: new[] { "text-medium date tokenized" }
                            ),
                        _DateTo: Shape.TextBox(
                            Id: "DateTo", Name: "DateTo",
                            Title: T("Activities to date"),
                            Description: T("Retrieve activities ending in or before this date"),
                            Classes: new[] { "text-medium date tokenized" }
                            ));
                    return f;
                };

            //_resourceManager.Value.Require("script", "jQuery");
            //_resourceManager.Value.Require("script", "jQueryUI_DatePicker");
            //_resourceManager.Value.Require("stylesheet", "jQueryUI_Orchard");
            //_resourceManager.Value.Require("stylesheet", "DateTimeEditor");
            //_resourceManager.Value.Include("script", "~/Modules/Laser.Orchard.Events/Scripts/datepicker.js", "~/Modules/Laser.Orchard.Events/Scripts/datepicker.js");

            context.Form("ActivityRangeForm", form);
        }
    }

    public class ActivityRangeFormValidator : IFormEventHandler
    {
        public Localizer T { get; set; }

        public void Building(BuildingContext context)
        {
        }

        public void Built(BuildingContext context)
        {
        }

        public void Validating(ValidatingContext context)
        {
            if (context.FormName != "ActivityRangeForm") return;

            if (string.IsNullOrEmpty(context.ValueProvider.GetValue("DateFrom").AttemptedValue) && string.IsNullOrEmpty(context.ValueProvider.GetValue("DateTo").AttemptedValue))
            {
                context.ModelState.AddModelError("ActivityRangeDateError", T("At least one date must be provided").Text);
            }
        }

        public void Validated(ValidatingContext context)
        {
        }
    }
}