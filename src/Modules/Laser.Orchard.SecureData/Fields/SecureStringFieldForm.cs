using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SecureData.Fields {
    public class SecureStringFieldForm : IFormProvider {
        public static string FormName = "SecureStringFieldForm";
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }

        public SecureStringFieldForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: FormName,
                        _FilterForm: Shape.TextBox(
                            Id: "Value",
                            Name: "Value",
                            Title: T("Value"),
                            Description: T("Secure value of the field."),
                            Classes: new[] { "text", "tokenized" }
                        ),
                        _ReturnAllElements: Shape.CheckBox(
                            Id: "ShowAllIfNoValue",
                            Name: "ShowAllIfNoValue",
                            Title: T("Return all elements if Value is empty."),
                            Description: T("Return all elements if no filter is provided.")
                        )
                    );
                    return f;
                };
            context.Form(FormName, form);
        }
    }
}