using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.ButtonToWorkflows.Activity {
    public class ButtonToWorkflowsForm : IFormProvider {
        protected dynamic _shapeFactory { get; set; }
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public Localizer T { get; set; }

        public ButtonToWorkflowsForm(IShapeFactory shapeFactory, IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _shapeFactory = shapeFactory;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {

            Func<IShapeFactory, dynamic> form =
                 shape => {

                     var f = _shapeFactory.Form(
                         Id: "_AnyOfContentTypes",
                         _Parts: _shapeFactory.SelectList(
                             Id: "contenttypes", Name: "ContentTypes",
                             Title: T("Content types"),
                             Description: T("Select some content types."),
                             Size: 10,
                             Multiple: true
                             )
                         );

                     f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                     foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions().OrderBy(x => x.DisplayName)) {
                         f._Parts.Add(new SelectListItem { Value = contentType.Name, Text = contentType.DisplayName });
                     }

                     return f;
                 };

            context.Form("_SelectContentTypes", form);

        }
    }
}