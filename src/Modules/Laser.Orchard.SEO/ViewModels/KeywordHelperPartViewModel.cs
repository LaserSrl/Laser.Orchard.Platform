using Laser.Orchard.SEO.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.SEO.ViewModels {
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperPartViewModel {
        /// <summary>
        /// List of keywords. Remember to use regular expressions in the drivers to clean them up
        /// </summary>
        public IEnumerable<KeywordHelperKeyword> Keywords { get; set; }

        /// <summary>
        /// Deafult ctor
        /// </summary>
        public KeywordHelperPartViewModel() {
            Keywords = new List<KeywordHelperKeyword>();
        }

        /// <summary>
        /// Constructor from a KeywordHelperPart
        /// </summary>
        /// <param name="part">The part we want to output.</param>
        public KeywordHelperPartViewModel(KeywordHelperPart part) {
            if (string.IsNullOrWhiteSpace(part.Keywords)) {
                Keywords = new List<KeywordHelperKeyword>();
            } else {
                Keywords = part
                    .Keywords
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .Select(x => new KeywordHelperKeyword(x));
            }
        }

        /// <summary>
        /// From the list of keywords, create a single string to store in the record.
        /// </summary>
        /// <returns>The comma-separated string representing the list of keywords.</returns>
        public string ListToString() {
            if (Keywords == null || Keywords.Where(k => !k.Delete).Count() == 0)
                return "";

            string parsed = String.Join(",", Keywords.Where(k => !k.Delete).Select(x=>x.CleanKeyword()));
            

            return parsed;
        }
    }

    //we are creating a class here, that for now only really wraps strings, just to simplify building views dynamically
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperKeyword {
        public string Keyword { get; set; }
        public bool Delete { get; set; }

        public static Dictionary<string, string> langDictionary;
        public static Dictionary<string, string> regionDictionary;

        public KeywordHelperKeyword() {
            Keyword = "";
        }

        public KeywordHelperKeyword(string k) {
            Keyword = k;
        }

        /// <summary>
        /// Trim and remove excessive whitespace
        /// </summary>
        /// <returns>A duplicate of the Keyword string, from which whitespaces have been removed</returns>
        public string CleanKeyword() {
            string clean = Regex.Replace(Keyword.Trim(), @"\s", " ");

            return clean;
        }

        /// <summary>
        /// Do percent encoding for the keyword
        /// </summary>
        /// <returns>The percent encoded version of the keyword</returns>
        public string PercentEncode() {
            string clean = this.CleanKeyword();
            return System.Web.HttpUtility.UrlEncode(clean);
        }
    }

    //This model is used to refresh the Google Trends charts
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class GoogleTrendsViewModel {

        public string hl { get; set; } //language
        public string q { get; set; } //keywords(percent encoded)
        public string geo { get; set; } //location
        public string date { get; set; } //time period
        
    }
}