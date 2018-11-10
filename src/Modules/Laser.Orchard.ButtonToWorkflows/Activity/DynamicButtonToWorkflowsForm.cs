using System;
using System.Linq;
using System.Web.Mvc;
using Laser.Orchard.ButtonToWorkflows.Services;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Laser.Orchard.ButtonToWorkflows.Activity {
    public class DynamicButtonToWorkflowsForm : IFormProvider {

        protected dynamic _shapeFactory { get; set; }

        private readonly IDynamicButtonToWorkflowsService _dynamicButtonToWorkflowsService;

        public Localizer T { get; set; }

        public DynamicButtonToWorkflowsForm(IShapeFactory shapeFactory, IDynamicButtonToWorkflowsService dynamicButtonToWorkflowsService) {
            _shapeFactory = shapeFactory;
            _dynamicButtonToWorkflowsService = dynamicButtonToWorkflowsService;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {

            Func<IShapeFactory, dynamic> form =
                 shape => {

                     var frm = _shapeFactory.Form(
                            Id: "_DynamicButtonSelect",
                            _Buttons: _shapeFactory.SelectList(
                                Id: "DynamicButton", Name: "DynamicButton",
                                Title: T("Dynamic Button"),
                                Description: T("Select a dynamic button."),
                                Size: 10,
                                Multiple: false
                            )
                         );

                     foreach (var contentType in _dynamicButtonToWorkflowsService.GetButtons().OrderBy(o => o.ButtonName)) {
                         var optionText = contentType.ButtonName;
                         if (!string.IsNullOrWhiteSpace(contentType.ButtonDescription))
                             optionText = optionText + " (" + contentType.ButtonDescription + ")";

                         frm._Buttons.Add(new SelectListItem { Value = contentType.GlobalIdentifier, Text = optionText });
                     }

                     return frm;

                     //var f = _shapeFactory.Form(
                     //    Id: "_AnyOfContentTypes",
                     //    _Parts: _shapeFactory.SelectList(
                     //        Id: "contenttypes", Name: "ContentTypes",
                     //        Title: T("Content types"),
                     //        Description: T("Select some content types."),
                     //        Size: 10,
                     //        Multiple: true
                     //        )
                     //    );

                     //f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                     //foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions().OrderBy(x => x.DisplayName)) {
                     //    f._Parts.Add(new SelectListItem { Value = contentType.Name, Text = contentType.DisplayName });
                     //}

                     //return f;
                 };

            context.Form("_DynamicButtonSelectForm", form);
        }
    }
}