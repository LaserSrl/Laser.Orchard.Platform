using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Laser.Orchard.Questionnaires.ViewModels.StatsSearchContext;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class StatsDetailFilterContextBase {
        public StatsDetailFilterContextBase() {
            DateFrom = null;
            DateTo = null;
            Context = null;
        }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Context { get; set; }
    }
    public class StatsDetailFilterContext : StatsDetailFilterContextBase {

        public StatsDetailFilterContext() {
            Export = false;
        }
        public bool Export { get; set; }
    }
}