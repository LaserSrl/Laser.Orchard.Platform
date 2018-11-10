using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;
using Orchard.Localization;
using Laser.Orchard.StartupConfig.ViewModels;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IActivityServices : IDependency {
        Response TriggerSignal(string signalName, int contentId);
        LocalizedString[] RequestInspectorWokflowOutcomes(InspectionType inspectionType);
        LocalizedString[] RequestInspectorWokflowOutcomes(string inspectionTypeString);
    }
}
