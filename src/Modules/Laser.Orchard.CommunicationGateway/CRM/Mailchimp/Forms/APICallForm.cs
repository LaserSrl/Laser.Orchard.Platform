using Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Services;
using Laser.Orchard.Policy.Services;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.CRM.Mailchimp.Forms {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class APICallForm : IFormProvider {
        protected dynamic Shape { get; set; }

        private readonly IPolicyServices _policyService;
        private readonly IMailchimpApiService _mailchimpApiService;

        public Localizer T { get; set; }

        public APICallForm(IShapeFactory shapeFactory, IPolicyServices policyService, IMailchimpApiService mailchimpApiService) {
            Shape = shapeFactory;
            _policyService = policyService;
            _mailchimpApiService = mailchimpApiService;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            context.Form("MailchimpAPICallForm",
                shape => Shape.Form(
                Id: "MailchimpAPICallForm",
                _Model : Shape.MailchimpAPICallEdit(
                        Id: "APICallEdit", Name: "APICallEdit",
                        Policies: _policyService.GetAllPublishedPolicyTexts().ToList(),
                        RequestTypes: _mailchimpApiService.GetRequestTypes(),
                        BaseUrl: _mailchimpApiService.GetBaseUrl()
                    )
                )
            );
        }
    }
    public class APICallFormsValidator : IFormEventHandler {
        public Localizer T { get; set; }

        public void Building(BuildingContext context) {
        }

        public void Built(BuildingContext context) {
        }

        public void Validating(ValidatingContext context) {
            if (context.FormName == "MailchimpAPICallForm") {
            }
        }

        public void Validated(ValidatingContext context) {

        }
    }


}