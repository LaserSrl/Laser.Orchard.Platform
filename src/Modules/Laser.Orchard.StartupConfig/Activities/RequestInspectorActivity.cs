using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Laser.Orchard.StartupConfig.Activities {
    public class RequestInspectorActivity : Task {
        private readonly IOrchardServices _orchardServices;
        private readonly IActivityServices _activityServices;
        private readonly ICommonsServices _commonServices;
        public RequestInspectorActivity(IOrchardServices orchardServices,
            IActivityServices activityServices, ICommonsServices commonServices) {
            _orchardServices = orchardServices;
            _activityServices = activityServices;
            _commonServices = commonServices;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public override string Form {
            get {
                return "RequestInspectorEditForm";
            }
        }

        public override LocalizedString Category {
            get { return T("Utils"); }
        }

        public override string Name {
            get { return "RequestInspector"; }
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext) {
            return true;
        }

        public override LocalizedString Description {
            get { return T("Depending on the type of inspection, generates the appropriate output."); }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            string inspectionTypeString = activityContext.GetState<string>("InspectionType");
            return _activityServices.RequestInspectorWokflowOutcomes(inspectionTypeString);
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            string inspectionTypeString = activityContext.GetState<string>("InspectionType");
            var inspectionType = InspectionType.DeviceBrand;
            Enum.TryParse(inspectionTypeString, out inspectionType);
            if (inspectionType == InspectionType.DeviceBrand) {
                var brand = _commonServices.GetDeviceBrandByUserAgent().ToString();
                yield return T(brand);
            } else {
                yield return T("Unknown");
            }
        }
    }
}