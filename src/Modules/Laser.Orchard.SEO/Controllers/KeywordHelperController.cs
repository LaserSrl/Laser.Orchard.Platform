using Laser.Orchard.SEO.ViewModels;
using Orchard.Environment.Extensions;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.SEO.Controllers {
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperController : Controller {

        [Admin]
        public ActionResult RefreshTrends(string _hl, string _q, string _geo, string _date) {
            string query = ParseQString(_q);

            var model = new GoogleTrendsViewModel {
                hl = _hl,
                q = _q,
                geo = _geo,
                date = _date
            };
            return PartialView((object)model);
        }

        [Admin]
        public ActionResult TabbedCharts(string _hl, string _q, string _geo, string _date) {
            string query = ParseQString(_q);

            var model = new GoogleTrendsViewModel {
                hl = _hl,
                q = query,
                geo = _geo,
                date = _date
            };
            return PartialView((object)model);
        }

        [Admin]
        public ActionResult SummaryTrends(string _hl, string _q, string _geo, string _date) {
            string query = ParseQString(_q);

            var model = new GoogleTrendsViewModel {
                hl = _hl,
                q = query,
                geo = _geo,
                date = _date
            };
            return PartialView((object)model);
        }

        private string ParseQString(string q) {
            string[] inWords = q.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> kWords = new List<string>();
            kWords.AddRange(inWords.Select(w => new KeywordHelperKeyword(w).PercentEncode()));
            return String.Join(",", kWords.Distinct());
        }
    }
}