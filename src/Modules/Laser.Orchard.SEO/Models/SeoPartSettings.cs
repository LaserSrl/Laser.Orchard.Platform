using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Models {
    public class SeoPartSettings {
        [Display(Name = "noindex")]
        public bool RobotsNoIndex { get; set; }
        [Display(Name = "nofollow")]
        public bool RobotsNoFollow { get; set; }
        [Display(Name = "nosnippet")]
        public bool RobotsNoSnippet { get; set; }
        [Display(Name = "noodp")]
        public bool RobotsNoOdp { get; set; }
        [Display(Name = "noarchive")]
        public bool RobotsNoArchive { get; set; }
        [Display(Name = "unavailable_after")]
        public bool RobotsUnavailableAfter { get; set; }
        [Display(Name = "noimageindex")]
        public bool RobotsNoImageIndex { get; set; }
        [Display(Name = "nositelinkssearchbox")]
        public bool GoogleNoSiteLinkSearchBox { get; set; }
        [Display(Name = "notranslate")]
        public bool GoogleNoTranslate { get; set; }

        [Display(Name = "JSON-LD")]
        public string JsonLd { get; set; }
        [Display(Name = "Templates")]
        public Dictionary<string, string> Templates;

        [Display(Name = "ShowAggregatedMicrodata")]
        public bool ShowAggregatedMicrodata { get; set; }

        [Display(Name = "CanonicalUrl")]
        public string CanonicalUrl { get; set; }
    }
}