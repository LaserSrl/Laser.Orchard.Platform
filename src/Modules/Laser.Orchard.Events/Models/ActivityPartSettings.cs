using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Events.Models {
    public class ActivityPartSettings {

        public bool UseRecurrences { get; set; }
        public bool SingleDate { get; set; }

        public DateTimeTypes DateTimeType { get; set; }

        public string DateStartLabel { get; set;}

        public string DateEndLabel { get; set; }

        public string ActivityPartLabel { get; set; }
    }
}