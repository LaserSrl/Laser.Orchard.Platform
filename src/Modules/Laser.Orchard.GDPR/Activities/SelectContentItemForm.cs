using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;

namespace Laser.Orchard.GDPR.Activities {
    [OrchardFeature("Laser.Orchard.GDPR.Workflows")]
    public class SelectContentItemForm : IFormProvider {

        protected dynamic Shape { get; set; }

        public SelectContentItemForm(
            IShapeFactory shapeFactory) {

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form = shape => {
                return Shape.Form(
                    Id: "GDPRActivityForm",
                    _Type: Shape.FieldSet(
                        Title: T("Select the ContentItem"),
                        _ContentItemId: Shape.Textbox(
                            Id: "GDPRActivityForm-ContentnItemId",
                            Name: "ContentItemId",
                            Title: T("ContentItem Id"),
                            Description: T("Specify the Id of the ContentItem to be subjected to a process for GDPR compliance. You may specify multiple Ids by separating tem with a comma. Leave this empty to attempt to use the ContentItem from the WorkflowContext."),
                            Classes: new[] { "large", "text", "tokenized" }
                            )
                        )
                    );
            };
            context.Form("GDPRSelectContentItem", form);
        }
    }
}