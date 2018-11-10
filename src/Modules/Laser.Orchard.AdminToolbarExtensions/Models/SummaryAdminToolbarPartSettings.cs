using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System.Text;
using System.Text.RegularExpressions;

namespace Laser.Orchard.AdminToolbarExtensions.Models {
    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions")]
    public class SummaryAdminToolbarPartSettings {
        private static string formatTemplate = "{{ Label={0}, Area={1}, Controller={2}, Action={3}, Parameters={4}, CustomUrl={5}, Target={6} }},";
        public IEnumerable<SummaryAdminToolbarLabel> Labels { get; set; }

        public SummaryAdminToolbarPartSettings() {
            Labels = new List<SummaryAdminToolbarLabel>();
        }

        public string ParseListToString() {
            if (Labels.Count() == 0) return "";

            StringBuilder sb = new StringBuilder();
            //if the Delete bool is set, we want to get rid of the Label
            foreach (SummaryAdminToolbarLabel lbl in Labels.Where(l => !l.Delete)) {
                object[] data = {lbl.Label, lbl.Area, lbl.Controller, lbl.Action, lbl.Parameters, lbl.CustomUrl, lbl.Target};
                sb.AppendFormat(formatTemplate, data );
            }

            return sb.ToString();
        }

        public void ParseStringToList(string toParse) {
            Labels = new List<SummaryAdminToolbarLabel>();
            string[] subsa = toParse.Split(new string[] { " },", "{ " }, StringSplitOptions.RemoveEmptyEntries);
            var subs = subsa.Where(s => !s.Equals(","));
            //found the following snippet on http://stackoverflow.com/a/5346288/2669614
            string pattern = "^" + Regex.Replace(formatTemplate, @"\{[0-9]+\}", "(.*?)") + "$";
            pattern = Regex.Replace(pattern, @"\{\{ ", "");
            pattern = Regex.Replace(pattern, @" \}\},", "");
            Regex r = new Regex(pattern);
            foreach (var sub in subs) {
                Match m = r.Match(sub);
                if (m.Groups.Count == 8) {
		            var lbl = new SummaryAdminToolbarLabel();
		            lbl.Label = m.Groups[1].Value;
		            lbl.Area = m.Groups[2].Value;
		            lbl.Controller = m.Groups[3].Value;
		            lbl.Action  = m.Groups[4].Value;
		            lbl.Parameters= m.Groups[5].Value;
		            lbl.CustomUrl = m.Groups[6].Value;
                    lbl.Target = (ValidLabelTargets)Enum.Parse(typeof(ValidLabelTargets), m.Groups[7].Value);
                    ((List<SummaryAdminToolbarLabel>)Labels).Add(lbl);
	            }
            }
        }
    }

    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions")]
    public class SummaryAdminToolbarLabel {
        public string Label { get; set; } //label that will be visualized in the SummaryAdmin view
        public string Area { get; set; } //Area parameter for dynamically computed action
        public string Controller { get; set; } //the controller called for the computed action
        public string Action { get; set; } //the action that will be called
        public string Parameters { get; set; } //parameters for the action
        public string CustomUrl { get; set; } //custom url used when the SummaryAdmin link is clicked. If null or empty, try to compute the action
        public ValidLabelTargets Target { get; set; } //_blank or _self

        public bool Delete { get; set; } //for editor window
    }


}