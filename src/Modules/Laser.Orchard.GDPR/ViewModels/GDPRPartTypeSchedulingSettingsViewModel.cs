using Laser.Orchard.GDPR.Extensions;
using Laser.Orchard.GDPR.Settings;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.GDPR.ViewModels {
    [OrchardFeature("Laser.Orchard.GDPR.Scheduling")]
    public class GDPRPartTypeSchedulingSettingsViewModel {

        public GDPRPartTypeSchedulingSettingsViewModel() {
            Settings = new GDPRPartTypeSchedulingSettings();
        }

        public GDPRPartTypeSchedulingSettings Settings { get; set; }

        public bool ScheduleAnonymization {
            get { return Settings.ScheduleAnonymization; }
            set { Settings.ScheduleAnonymization = value; }
        }

        public EventForScheduling EventForAnonymization {
            get { return Settings.EventForAnonymization; }
            set { Settings.EventForAnonymization = value; }
        }

        public int AnonymizationDaysToWait {
            get { return Settings.AnonymizationDaysToWait; }
            set { Settings.AnonymizationDaysToWait = value; }
        }
        
        public IEnumerable<SelectListItem> AnonymizationEvents { get; set; }

        public bool ScheduleErasure {
            get { return Settings.ScheduleErasure; }
            set { Settings.ScheduleErasure = value; }
        }

        public EventForScheduling EventForErasure {
            get { return Settings.EventForErasure; }
            set { Settings.EventForErasure = value; }
        }

        public int ErasureDaysToWait {
            get { return Settings.ErasureDaysToWait; }
            set { Settings.ErasureDaysToWait = value; }
        }

        public IEnumerable<SelectListItem> ErasureEvents { get; set; }
    }
}