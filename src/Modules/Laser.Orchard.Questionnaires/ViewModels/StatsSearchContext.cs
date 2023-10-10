using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Laser.Orchard.Questionnaires.ViewModels.StatsSearchContext;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class StatsSearchContext {
        public string SearchText { get; set; }
        public SearchTypeOptions SearchType { get; set; }

        public enum SearchTypeOptions {
            Contents,
            Widgets
        }
    }

    public static class StatsSearchContextExtensions {
        public static LocalizedString Text(this SearchTypeOptions searchType, Localizer T) {
            if (searchType == SearchTypeOptions.Contents) {
                return T("Contents");
            }
            else if (searchType == SearchTypeOptions.Widgets) {
                return T("Widgets");
            }
            return null;

        }

    }

}