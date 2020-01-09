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
using Laser.Orchard.StartupConfig.ViewModels;

namespace Laser.Orchard.StartupConfig.Activities {
    public class WebApiResponseEditForm : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public WebApiResponseEditForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "WebApiResponseEditForm",
                        _Type: Shape.FieldSet(
                            Title: T("Response"),
                            _Success: Shape.Checkbox(
                                Id: "successful", Name: "Successful",
                                Title: T("Success"),
                                Checked: false, Value: "true",
                                Description: T("Check if the the activity should return a success response.")),
                            _Subject: Shape.Textbox(
                                Id: "message", Name: "Message",
                                Title: T("Message"),
                                Description: T("The text message of the response."),
                                Classes: new[] { "large", "text", "tokenized" }),
                            _Data: Shape.Textbox(
                                Id: "data", Name: "Data",
                                Title: T("Data"),
                                Description: T("The data object of the response, the data must be in Json format es: {{\"ErrorType\":\"TicketError\"}}. "),
                                Classes: new[] { "large", "text", "tokenized" }),
                            _ErrorCode: Shape.SelectList(
                                Id: "error-code",
                                Name: "ErrorCode",
                                Title: T("Error Code"),
                                Description: ("The error code.")
                            ),
                            _ResolutionAction: Shape.SelectList(
                                Id: "resolution-action",
                                Name: "ResolutionAction",
                                Title: T("Resolution Action"),
                                Description: ("The suggested resolution action.")
                            )
                        ));

                    foreach (var errorCode in Enum.GetNames(typeof(ErrorCode))) {
                        f._Type._ErrorCode.Add(new SelectListItem { Value = errorCode, Text = errorCode });
                    }
                    foreach (var resolutionAction in Enum.GetNames(typeof(ResolutionAction))) {
                        f._Type._ResolutionAction.Add(new SelectListItem { Value = resolutionAction, Text = resolutionAction });
                    }

                    return f;
                };
            context.Form("WebApiResponseEditForm", form);
        }
    }
}