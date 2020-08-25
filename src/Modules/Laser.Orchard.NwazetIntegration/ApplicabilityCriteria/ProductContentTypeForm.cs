using Nwazet.Commerce.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class ProductContentTypeForm : IFormProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public static string FormName = "ProductContentTypeForm";


        public ProductContentTypeForm(
            IShapeFactory shapeFactory,
            IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "AnyOfContentTypes",
                        _Parts: Shape.SelectList(
                            Id: "contenttypes", Name: "ContentTypes",
                            Title: T("Content types"),
                            Description: T("Select some content types."),
                            Size: 10,
                            Multiple: true
                            )
                        );
                    // We don't add an "Any" option
                    //f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    foreach (var contentType in _contentDefinitionManager
                        .ListTypeDefinitions()
                        // Type has ProductPart
                        .Where(ctd => ctd.Parts.Any(ctpd => ctpd
                            .PartDefinition.Name
                            .Equals(ProductPart.PartName, StringComparison.InvariantCultureIgnoreCase)))
                        .OrderBy(x => x.DisplayName)) {

                        f._Parts.Add(new SelectListItem {
                            Value = contentType.Name,
                            Text = contentType.DisplayName });
                    }

                    return f;
                };

            context.Form(FormName, form);

        }
    }
}