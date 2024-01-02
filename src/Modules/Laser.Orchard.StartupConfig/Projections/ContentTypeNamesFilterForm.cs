using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using FormServices = Orchard.Forms.Services;

namespace Laser.Orchard.StartupConfig.Projections {
    public class ContentTypeNamesFilterForm : FormServices.IFormProvider {
        protected dynamic Shape { get; set; }

        public ContentTypeNamesFilterForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T= NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public const string FormName = "ContentTypeNamesFilterForm";

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form = shape => {
                var f = Shape.Form(
                    Id: FormName,
                    _ContentTypeNames: Shape.TextBox(
                        Id: "contenttypenames",
                        Name: "ContentTypeNames",
                        Title: T("Content type names"),
                        Classes: new[] { "text medium tokenized" },
                        Description: T("The content type names. If no name is provided, all allowed content types will be used.")),
                    _AllowedContentTypes: Shape.TextBox(
                        Id: "allowedcontenttypes",
                        Name: "AllowedContentTypes",
                        Title: T("Allowed content types"),
                        Classes: new[] { "text medium tokenized" },
                        Description: T("The allowed content type names. If no name is provided, every content type will be allowed"))
                    );
                return f;
            };
            context.Form(FormName, form);
        }
    }
}