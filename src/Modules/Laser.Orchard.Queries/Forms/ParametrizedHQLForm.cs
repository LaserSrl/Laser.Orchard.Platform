using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Queries.Forms {
    [OrchardFeature("Laser.Orchard.HQLProjectionFilter")]
    public class ParametrizedHQLForm : IFormProvider {

        protected dynamic Shape { get; set; }

        public ParametrizedHQLForm(
            IShapeFactory shapeFactory) {

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public const string FormName = "ParametrizedHQLForm";

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form = shape => {
                var f = Shape.Form(
                    Id: "ParametrizedHQLForm",
                    _Parameters: Shape.TextArea(
                            Id: "parameters", 
                            Name: "Parameters",
                            Title: T("Parameters"),
                            Classes: new[] { "tokenized" },
                            Description: T("Enter the values for the parameters. You may use tokens. Enter one value per line, and end each line with a comma: they will each map to a different parameter. This does not support multiline string parameters. All parameters are parsed as strings.")),
                    _Query: Shape.TextArea(
                            Id: "query", 
                            Name: "Query",
                            Title: T("HQL Query"),
                            Classes: new[] { "tokenized" },
                            Description: T("Enter the HQL query. You may use parameters with the following syntax: ( :paramN ). N is the line number (0-based) for the parameter configuration above. The parentheses and whitespace around :paramN are mandatory. The query should return the Ids of the selected ContentItems."))
                    );

                return f;
            };

            context.Form(FormName, form);
        }
    }
}