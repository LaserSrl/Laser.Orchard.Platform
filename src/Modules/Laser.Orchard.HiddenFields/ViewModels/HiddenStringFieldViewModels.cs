using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Services;
using Laser.Orchard.HiddenFields.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.HiddenFields.ViewModels {

    public class HiddenStringFieldSettingsEventsViewModel {

        public HiddenStringFieldSettings Settings { get; set; }

        public HiddenStringFieldUpdateProcessVariant ProcessVariant { get; set; }
        public string ProcessVariantString {
            get { return ProcessVariant.ToString(); }
            set { ProcessVariant = (HiddenStringFieldUpdateProcessVariant)Enum.Parse(
                    typeof(HiddenStringFieldUpdateProcessVariant), value); }
        }

        public IEnumerable<SelectListItem> ProcessVariants { get; set; }
    }

    public class HiddenStringFieldDriverViewModel {

        public bool IsEditAuthorized { get; set; }

        public HiddenStringField Field { get; set; }

        public string Value { get; set; }

        public HiddenStringFieldSettings Settings { get; set; }
    }
}