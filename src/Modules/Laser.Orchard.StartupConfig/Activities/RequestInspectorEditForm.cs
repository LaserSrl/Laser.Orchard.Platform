using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.Activities {
    public class RequestInspectorEditForm : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public RequestInspectorEditForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "RequestInspector",
                        _Type: Shape.FieldSet(
                            Title: T("Inspection Type"),
                            _DeviceType: Shape.Radio(
                                Id: "device-type",
                                Name: "InspectionType",
                                Value: "Device",
                                Title: T("Calling device"),
                                Description: T("Returns the calling device name, if known")
                            ),
                            _DeviceBrand: Shape.Radio(
                                Id: "device-store",
                                Name: "InspectionType",
                                Value: "DeviceBrand",
                                Title: T("Calling device brand"),
                                Description: T("Returns calling device brand, if known")
                            )
                        ));
                    return f;
                };
            context.Form("RequestInspectorEditForm", form);
        }
    }

    public class RequestInspectorEditFormValidator : IFormEventHandler {
        public Localizer T { get; set; }

        public void Building(BuildingContext context) {
        }

        public void Built(BuildingContext context) {
        }

        public void Validating(ValidatingContext context) {
        }

        public void Validated(ValidatingContext context) {
        }
    }

}