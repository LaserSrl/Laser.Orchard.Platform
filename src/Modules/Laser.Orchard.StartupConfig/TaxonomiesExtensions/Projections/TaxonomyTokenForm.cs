using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using Laser.Orchard.StartupConfig.Projections;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Projections
{

    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TaxonomyTokenForm : StartupConfig.Projections.IFormProvider {
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }

        public TaxonomyTokenForm(IShapeFactory shapeFactory)
        {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context)
        {
            Func<IShapeFactory, object> form =
                shape =>
                {
                    var f = Shape.Form(
                        Id: "SelectTermsByTokenForm",
                        _Token: Shape.TextBox(
                            Id: "TermToken", Name: "TermToken",
                            Title: T("Source Token"),
                            Description: T("The token used as a source for term identifiers"),
                            Classes: new[] { "text-medium tokenized" }
                            ),
                        _Exclusion: Shape.FieldSet(
                            _OperatorOneOf: Shape.Radio(
                                Id: "operator-is-one-of", Name: "Operator",
                                Title: T("Is associated to one of the specified terms"), Value: "0", Checked: true
                                ),
                            _OperatorIsAllOf: Shape.Radio(
                                Id: "operator-is-all-of", Name: "Operator",
                                Title: T("Is associated to all the specified terms"), Value: "1"
                                )
                            ),
                        _IncludeChildren: Shape.Checkbox(
                            Id: "IncludeChildren", Name: "IncludeChildren",
                            Title: T("Automatically include children terms in filtering"),
                            Value: "true"
                        )
                      );
                    return f;
                };

            context.Form("SelectTermsByTokenForm", form);
        }
    }

    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
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
        }

        public void Validated(ValidatingContext context)
        {
        }
    }
}