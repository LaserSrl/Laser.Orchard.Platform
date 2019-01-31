using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;

namespace Laser.Orchard.CulturePicker.Projections {
    public class SelectCultureForm : IFormProvider {

        protected dynamic Shape { get; set; }

        public SelectCultureForm(
            IShapeFactory shapeFactory) {

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public const string FormName = "SelectCultureForm";

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form = shape => {
                var f = Shape.Form(
                    Id: "SelectCultureForm",
                    _Parameters: Shape.TextBox(
                            Id: "culture",
                            Name: "Culture",
                            Title: T("Culture"),
                            Classes: new[] { "text medium tokenized" },
                            Description: T("Insert the desired culture name, in the usual form (e.g. it-IT). If the value does not match any configured culture, all content items will be included in the results. You may use tokens, but be careful as it's possible to accidentally set the culture of the WorkContext, for example with the 'lang' querystring parameter."))
                    );
                return f;
            };

            context.Form(FormName, form);
        }
    }
}