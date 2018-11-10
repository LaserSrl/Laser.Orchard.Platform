using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Workflows.Activities;
using Orchard.Workflows.Services;
using Orchard.Logging;
using Laser.Orchard.StartupConfig.ViewModels;

namespace Laser.Orchard.StartupConfig.Services {
    public class ActivityServices : IActivityServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkflowManager _workflowManager;
        private readonly IUtilsServices _utilsServices;
        public Localizer T { get; set; }
        public ILogger Log { get; set; }

        public ActivityServices(IOrchardServices orchardServices,
            IWorkflowManager workflowManager,
            IUtilsServices utilsServices) {
            _orchardServices = orchardServices;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
            _workflowManager = workflowManager;
            Log = NullLogger.Instance;
        }

        public Response TriggerSignal(string signalName, int contentId) {
            Response triggerResult = _utilsServices.GetResponse(ResponseType.Success);
            try {
                var content = _orchardServices.ContentManager.Get(contentId, VersionOptions.Published);
                var tokens = new Dictionary<string, object> { 
            { "Content", content }, 
            { SignalActivity.SignalEventName, signalName },
            {"WebApiResponse", triggerResult}
                };
                _workflowManager.TriggerEvent(SignalActivity.SignalEventName, content, () => tokens);
            } catch (Exception ex) {
                Log.Error("TriggerSignal " + ex.Message + "stack" + ex.StackTrace);
            }
            return triggerResult;
        }

        public LocalizedString[] RequestInspectorWokflowOutcomes(string inspectionTypeString) {
            var inspectionType = InspectionType.DeviceBrand;
            Enum.TryParse(inspectionTypeString, out inspectionType);
            return RequestInspectorWokflowOutcomes(inspectionType);
        }
        public LocalizedString[] RequestInspectorWokflowOutcomes(InspectionType inspectionType) {
            if (inspectionType == InspectionType.Device) { } else if (inspectionType == InspectionType.DeviceBrand) {
                var strings = Enum.GetValues(typeof(DevicesBrands)).Cast<DevicesBrands>().Select(s => T(s.ToString()));
                return strings.ToArray();
            }

            return new[] { T("Unknown") };

        }



    }
}