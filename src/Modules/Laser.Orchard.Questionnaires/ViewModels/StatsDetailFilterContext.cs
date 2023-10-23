using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Laser.Orchard.Questionnaires.ViewModels.StatsSearchContext;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class StatsDetailFilterContext{
        public StatsDetailFilterContext() {
            DateFrom = null;
            DateTo = null;
            Context = null;
        }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Context { get; set; }
        public bool Export { get; set; }
    }
}