using Orchard;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment;
using Orchard.Fields.Fields;
using OrchardForms = Orchard.Forms.Services;
using Orchard.Forms.Shapes;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;
using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Orchard.Localization.Services;

namespace Laser.Orchard.StartupConfig.Projections {
    public class DateTimeFilterForm : OrchardForms.IFormProvider {
        public const string FormName = "DateTimeFilterForm";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }
        protected LocalizedString _dateFromHint { get; set; }
        protected LocalizedString _dateToHint { get; set; }
        protected LocalizedString _operatorHint { get; set; }
        protected string _formName { get; set; }
        private readonly Work<IResourceManager> _resourceManager;

        public DateTimeFilterForm(IShapeFactory shapeFactory,
            Work<IResourceManager> resourceManager,
            IWorkContextAccessor workContextAccessor) {
            Shape = shapeFactory;
            _resourceManager = resourceManager;
            T = NullLocalizer.Instance;

            _dateFromHint = T("Select date and time the coupon begins to be valid");
            _dateToHint = T("Select date and time the coupon ends to be valid");
            _operatorHint = T("Select how do you want to apply the date and time validity of the coupon");

            _formName = FormName;
        }

        public void Describe(OrchardForms.DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "DateTimeFilterForm",
                        _Op: Shape.FieldSet(
                            Id: "fieldset-operator",
                            Title: "Operator",
                            _Operator: Shape.SelectList(
                                Id: "operator",
                                Name: "Operator",
                                Title: T("Operator"),
                                Size: 1,
                                Multiple: false,
                                EnableWrapper: false,
                                Description: _operatorHint
                            )
                        ),
                        // Property names must start with "_" to be added to the Items collection of the Shape.
                        _From: Shape.DateTimeEditorForm(
                            Id: "div-from",
                            Title: "From",
                            Hint: _dateFromHint,
                            _Date: Shape.TextBox(
                                    Id: "datefrom",
                                    Name: "DateFrom",
                                    EnableWrapper: false
                                ),
                            _Time: Shape.TextBox(
                                    Id: "timefrom",
                                    Name: "TimeFrom",
                                    EnableWrapper: false
                                )
                        ),
                        _To: Shape.DateTimeEditorForm(
                            Id: "div-to",
                            Title: "To",
                            Hint: _dateToHint,
                            _Date: Shape.TextBox(
                                    Id: "dateto",
                                    Name: "DateTo",
                                    EnableWrapper: false
                                ),
                            _Time: Shape.TextBox(
                                    Id: "timeto",
                                    Name: "TimeTo",
                                    EnableWrapper: false
                                )
                        )
                    );

                    _resourceManager.Value.Require("stylesheet", "jQueryCalendars_Picker");
                    _resourceManager.Value.Require("stylesheet", "jQueryTimeEntry");
                    _resourceManager.Value.Require("stylesheet", "jQueryDateTimeEditor");

                    _resourceManager.Value.Require("script", "jQuery");

                    _resourceManager.Value.Require("script", "jQueryCalendars_Picker").AtFoot();
                    _resourceManager.Value.Require("script", "JQueryTimeEntry").AtFoot();

                    _resourceManager.Value.Include("script", "~/Modules/Laser.Orchard.StartupConfig/Scripts/datetime-editor-filter.js", "~/Modules/Laser.Orchard.StartupConfig/Scripts/datetime-editor-filter.js").AtFoot();

                    f._Op._Operator.Add(new SelectListItem { Value = Convert.ToString(DateTimeOperator.LessThan), Text = T("Is less than").Text });
                    f._Op._Operator.Add(new SelectListItem { Value = Convert.ToString(DateTimeOperator.Between), Text = T("Is between").Text });
                    f._Op._Operator.Add(new SelectListItem { Value = Convert.ToString(DateTimeOperator.GreaterThan), Text = T("Is greater than").Text });

                    return f;
                };

            context.Form(FormName, form);
        }
    }

    public class DateTimeEditorShapes : IShapeTableProvider {
        private readonly ITagBuilderFactory _tagBuilderFactory;

        public DateTimeEditorShapes(ITagBuilderFactory tagBuilderFactory) {
            _tagBuilderFactory = tagBuilderFactory;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("DateTimeEditorForm").OnCreating(ctx => ctx.Create = () => new PropertiesAreItems());
        }

        // Shape is defined in DateTimeEditorForm.cshtml
    }

    public class DateTimeFilterFormValidator : OrchardForms.IFormEventHandler {

        public Localizer T { get; set; }
        private readonly IDateLocalizationServices _dateLocalizationServices;

        public DateTimeFilterFormValidator(IDateLocalizationServices dateLocalizationServices) {
            _dateLocalizationServices = dateLocalizationServices;
            T = NullLocalizer.Instance;
        }

        public void Validating(OrchardForms.ValidatingContext context) {
            if (context.FormName != DateTimeFilterForm.FormName) return;

            var op = (DateTimeOperator)Enum.Parse(typeof(DateTimeOperator), Convert.ToString(context.ValueProvider.GetValue("Operator").AttemptedValue));

            string dateFrom = context.ValueProvider.GetValue("DateFrom").AttemptedValue;
            string timeFrom = context.ValueProvider.GetValue("TimeFrom").AttemptedValue;
            string dateTo = context.ValueProvider.GetValue("DateTo").AttemptedValue;
            string timeTo = context.ValueProvider.GetValue("TimeTo").AttemptedValue;
            DateTime from = DateTime.UtcNow;
            DateTime to = DateTime.UtcNow;

            switch (op) {
                case DateTimeOperator.Between:
                    bool parseOk = true;

                    try {
                        from = _dateLocalizationServices.ConvertFromLocalizedString(dateFrom, timeFrom).Value;
                        } catch {
                        context.ModelState.AddModelError("DateFrom", T("Invalid dates and times").Text);
                        parseOk = false;
                    }

                    try {
                        to = _dateLocalizationServices.ConvertFromLocalizedString(dateTo, timeTo).Value;
                    } catch {
                        context.ModelState.AddModelError("DatTo", T("Invalid dates and times").Text);
                        parseOk = false;
                    }

                    if (parseOk && from > to) {
                        context.ModelState.AddModelError("DateFrom", T("First date must be before second date").Text);
                    }

                    break;

                case DateTimeOperator.LessThan:
                    try {
                        to = _dateLocalizationServices.ConvertFromLocalizedString(dateTo, timeTo).Value;
                    } catch {
                        context.ModelState.AddModelError("DateTo", T("Invalid dates and times").Text);
                    }

                    break;

                case DateTimeOperator.GreaterThan:
                    try {
                        from = _dateLocalizationServices.ConvertFromLocalizedString(dateFrom, timeFrom).Value;
                    } catch {
                        context.ModelState.AddModelError("DateFrom", T("Invalid dates and times").Text);
                    }

                    break;

                default:
                    context.ModelState.AddModelError("Operator", T("Invalid operator").Text);
                    break;
            }

            //context.ValueProvider.GetValue("DateFrom").AttemptedValue = from.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern);

        }

        public void Building(OrchardForms.BuildingContext context) {
            if (context.Shape.Id == "DateTimeFilterForm") {
                // Default values for dates and times is Now.
                context.Shape._From._Date.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern);
                context.Shape._From._Time.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortTimePattern);

                context.Shape._To._Date.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern);
                context.Shape._To._Time.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortTimePattern);
            }
        }

        public void Built(OrchardForms.BuildingContext context) { }

        public void Validated(OrchardForms.ValidatingContext context) {
            if (context.FormName != DateTimeFilterForm.FormName) return;

            
        }
    }
}
