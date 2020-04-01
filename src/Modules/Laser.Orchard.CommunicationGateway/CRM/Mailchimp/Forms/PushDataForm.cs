using Laser.Orchard.Policy.Services;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Mailchimp.Forms {
    [OrchardFeature("Laser.Orchard.CommunicationGateway.Mailchimp")]
    public class PushDataForm : IFormProvider {
        protected dynamic Shape { get; set; }

        private readonly IPolicyServices _policyService;

        public Localizer T { get; set; }

        public PushDataForm(IShapeFactory shapeFactory, IPolicyServices policyService) {
            Shape = shapeFactory;
            _policyService = policyService;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            context.Form("PushDataForm",
                shape => Shape.Form(
                Id: "PushDataForm",
                _Model : Shape.PushDataEdit(
                        Id: "pushdataedit", Name: "PushDataEdit",
                        Policies: _policyService.GetAllPublishedPolicyTexts().ToList()
                    )
                )
            );
        }
    }
}