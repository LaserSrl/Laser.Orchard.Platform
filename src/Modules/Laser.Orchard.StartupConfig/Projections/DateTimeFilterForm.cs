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
        private readonly Lazy<CultureInfo> _currentCulture;

        public DateTimeFilterForm(IShapeFactory shapeFactory,
            Work<IResourceManager> resourceManager,
            IWorkContextAccessor workContextAccessor) {
            Shape = shapeFactory;
            _resourceManager = resourceManager;
            _currentCulture = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(workContextAccessor.GetContext().CurrentCulture));
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
                            ),
                            // I need to declare culture inside the DateTimeEditorForm to properly manage it in the razor.
                            _Culture: Shape.Hidden(
                                Id: "culturefrom",
                                Name: "CultureFrom",
                                EnableWrapper: false,
                                Value: _currentCulture
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
                           ),
                            // I need to declare culture inside the DateTimeEditorForm to properly manage it in the razor.
                           _Culture: Shape.Hidden(
                                Id: "cultureto",
                                Name: "CultureTo",
                                EnableWrapper: false,
                                Value: _currentCulture
                            )
                        )
                    );

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
        private readonly IWorkContextAccessor _workContextAccessor;

        public DateTimeFilterFormValidator(IDateLocalizationServices dateLocalizationServices,
                IWorkContextAccessor workContextAccessor) {
            _dateLocalizationServices = dateLocalizationServices;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public void Validating(OrchardForms.ValidatingContext context) {
            if (context.FormName != DateTimeFilterForm.FormName) return;

            var op = (DateTimeOperator)Enum.Parse(typeof(DateTimeOperator), Convert.ToString(context.ValueProvider.GetValue("Operator").AttemptedValue));

            string dateFrom = context.ValueProvider.GetValue("DateFrom").AttemptedValue;
            string timeFrom = context.ValueProvider.GetValue("TimeFrom").AttemptedValue;
            string dateTo = context.ValueProvider.GetValue("DateTo").AttemptedValue;
            string timeTo = context.ValueProvider.GetValue("TimeTo").AttemptedValue;
            var cultureFrom = CultureInfo.GetCultureInfo(context.ValueProvider.GetValue("CultureFrom").AttemptedValue);
            var cultureTo = CultureInfo.GetCultureInfo(context.ValueProvider.GetValue("CultureTo").AttemptedValue);
            var utcNow = DateTime.UtcNow;
            DateTime from = _dateLocalizationServices.ConvertToSiteTimeZone(utcNow);
            DateTime to = _dateLocalizationServices.ConvertToSiteTimeZone(utcNow);

            switch (op) {
                case DateTimeOperator.Between:
                    bool parseOk = true;

                    try {
                        from = Convert.ToDateTime(dateFrom + " " + timeFrom, cultureFrom);
                        } catch {
                        context.ModelState.AddModelError("DateFrom", T("Invalid dates and times").Text);
                        parseOk = false;
                    }

                    try {
                        to = Convert.ToDateTime(dateTo + " " + timeTo, cultureTo);
                    } catch {
                        context.ModelState.AddModelError("DateTo", T("Invalid dates and times").Text);
                        parseOk = false;
                    }

                    if (parseOk && from > to) {
                        context.ModelState.AddModelError("DateFrom", T("First date must be before second date").Text);
                    }

                    break;

                case DateTimeOperator.LessThan:
                    try {
                        to = Convert.ToDateTime(dateTo + " " + timeTo, cultureTo);
                    } catch {
                        context.ModelState.AddModelError("DateTo", T("Invalid dates and times").Text);
                    }

                    break;

                case DateTimeOperator.GreaterThan:
                    try {
                        from = Convert.ToDateTime(dateFrom + " " + timeFrom, cultureFrom);
                    } catch {
                        context.ModelState.AddModelError("DateFrom", T("Invalid dates and times").Text);
                    }

                    break;

                default:
                    context.ModelState.AddModelError("Operator", T("Invalid operator").Text);
                    break;
            }
        }

        public void Building(OrchardForms.BuildingContext context) {
            if (context.Shape.Id == "DateTimeFilterForm") {
                var siteCulture = CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture);
                var utcNow = DateTime.UtcNow;

                // Default values for dates and times is UtcNow.
                context.Shape._From._Date.Value = _dateLocalizationServices.ConvertToSiteTimeZone(utcNow).ToString(siteCulture.DateTimeFormat.ShortDatePattern);
                context.Shape._From._Time.Value = _dateLocalizationServices.ConvertToSiteTimeZone(utcNow).ToString(siteCulture.DateTimeFormat.ShortTimePattern);
                context.Shape._From._Culture.Value = siteCulture.Name;

                context.Shape._To._Date.Value = _dateLocalizationServices.ConvertToSiteTimeZone(utcNow).ToString(siteCulture.DateTimeFormat.ShortDatePattern);
                context.Shape._To._Time.Value = _dateLocalizationServices.ConvertToSiteTimeZone(utcNow).ToString(siteCulture.DateTimeFormat.ShortTimePattern);
                context.Shape._To._Culture.Value = siteCulture.Name;
            }
        }

        public void Built(OrchardForms.BuildingContext context) { }

        public void Validated(OrchardForms.ValidatingContext context) { }
    }
}
