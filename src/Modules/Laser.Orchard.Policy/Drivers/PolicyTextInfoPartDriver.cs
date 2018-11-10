using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.Policy.Drivers {
    public class PolicyTextInfoPartDriver : ContentPartCloningDriver<PolicyTextInfoPart> {
        private readonly IPolicyServices _policyServices;
        private readonly IOrchardServices _orchardServices;
        private readonly RequestContext _requestContext;

        public PolicyTextInfoPartDriver(IOrchardServices orchardServices, RequestContext requestContext, IPolicyServices policyServices) {
            _policyServices = policyServices;
            _orchardServices = orchardServices;
            _requestContext = requestContext;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Policy"; }
        }

        protected override DriverResult Display(PolicyTextInfoPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_PolicyTextInfo_SummaryAdmin",
                    () => shapeHelper.Parts_PolicyTextInfo_SummaryAdmin(PolicyTextInfoPart: part));
            }
            if (displayType == "Summary") {
                return ContentShape("Parts_PolicyTextInfo_Summary",
                        () => shapeHelper.Parts_PolicyTextInfo_Summary(PolicyTextInfoPart: part));
            }
            if (displayType == "Detail") {
                return ContentShape("Parts_PolicyTextInfo",
                        () => shapeHelper.Parts_PolicyTextInfo(PolicyTextInfoPart: part));
            }
            return null;
        }
        protected override DriverResult Editor(PolicyTextInfoPart part, dynamic shapeHelper) {
            return ContentShape("Parts_PolicyTextInfo_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/PolicyTextInfo_Edit",
                                 Model: part,
                                 Prefix: Prefix));
        }

        protected override DriverResult Editor(PolicyTextInfoPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("PolicyTextInfoPartError", T("PolicyTextInfo Error"));
            }
            return Editor(part, shapeHelper);
        }


        #region [ Import/Export ]
        protected override void Exporting(PolicyTextInfoPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("PolicyType", part.PolicyType);
            root.SetAttributeValue("Priority", part.Priority);
            root.SetAttributeValue("UserHaveToAccept", part.UserHaveToAccept);
        }

        protected override void Importing(PolicyTextInfoPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);

            var policyType = PolicyTypeOptions.Policy;
            var priority = 0;
            var userHaveToAccept = false;

            if(Enum.TryParse<PolicyTypeOptions>(root.Attribute("PolicyType").Value, out policyType)) {
                part.PolicyType = policyType;
            }
            if(Int32.TryParse(root.Attribute("Priority").Value, out priority)) {
                part.Priority = priority;
            }
            if(bool.TryParse(root.Attribute("UserHaveToAccept").Value, out userHaveToAccept)) {
                part.UserHaveToAccept = userHaveToAccept;
            }
        }
        #endregion

        protected override void Cloning(PolicyTextInfoPart originalPart, PolicyTextInfoPart clonePart, CloneContentContext context) {
            clonePart.UserHaveToAccept = originalPart.UserHaveToAccept;
            clonePart.Priority = originalPart.Priority;
            clonePart.PolicyType = originalPart.PolicyType;
        }
    }
}