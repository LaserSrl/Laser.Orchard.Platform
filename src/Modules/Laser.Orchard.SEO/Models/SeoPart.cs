using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using System;

namespace Laser.Orchard.SEO.Models {

    public class SeoPart : ContentPart<SeoVersionRecord> {

        public string TitleOverride {
            get { return this.Retrieve(x => x.TitleOverride); }
            set { this.Store(x => x.TitleOverride, value); }
        }

        public string Keywords {
            get { return this.Retrieve(x => x.Keywords); }
            set { this.Store(x => x.Keywords, value); }
        }

        public string Description {
            get { return this.Retrieve(x => x.Description); }
            set { this.Store(x => x.Description, value); }
        }

        [Display(Name = "noindex")]
        public bool RobotsNoIndex {
            get { return this.Retrieve(x => x.RobotsNoIndex); }
            set { this.Store(x => x.RobotsNoIndex, value); }
        }
        [Display(Name = "nofollow")]
        public bool RobotsNoFollow {
            get { return this.Retrieve(x => x.RobotsNoFollow); }
            set { this.Store(x => x.RobotsNoFollow, value); }
        }
        [Display(Name = "nosnippet")]
        public bool RobotsNoSnippet {
            get { return this.Retrieve(x => x.RobotsNoSnippet); }
            set { this.Store(x => x.RobotsNoSnippet, value); }
        }
        [Display(Name = "noodp")]
        public bool RobotsNoOdp {
            get { return this.Retrieve(x => x.RobotsNoOdp); }
            set { this.Store(x => x.RobotsNoOdp, value); }
        }
        [Display(Name = "noarchive")]
        public bool RobotsNoArchive {
            get { return this.Retrieve(x => x.RobotsNoArchive); }
            set { this.Store(x => x.RobotsNoArchive, value); }
        }
        [Display(Name = "unavailable_after")]
        public bool RobotsUnavailableAfter {
            get { return this.Retrieve(x => x.RobotsUnavailableAfter); }
            set { this.Store(x => x.RobotsUnavailableAfter, value); }
        }
        public DateTime? RobotsUnavailableAfterDate {
            get { return this.Retrieve(x => x.RobotsUnavailableAfterDate); }
            set { this.Store(x => x.RobotsUnavailableAfterDate, value); }
        }
        [Display(Name = "noimageindex")]
        public bool RobotsNoImageIndex {
            get { return this.Retrieve(x => x.RobotsNoImageIndex); }
            set { this.Store(x => x.RobotsNoImageIndex, value); }
        }
        [Display(Name = "nositelinkssearchbox")]
        public bool GoogleNoSiteLinkSearchBox {
            get { return this.Retrieve(x => x.GoogleNoSiteLinkSearchBox); }
            set { this.Store(x => x.GoogleNoSiteLinkSearchBox, value); }
        }
        [Display(Name = "notranslate")]
        public bool GoogleNoTranslate {
            get { return this.Retrieve(x => x.GoogleNoTranslate); }
            set { this.Store(x => x.GoogleNoTranslate, value); }
        }
        public bool HideDetailMicrodata {
            get { return this.Retrieve(x => x.HideDetailMicrodata, false); }
            set { this.Store(x => x.HideDetailMicrodata, value); }
        }
        public bool HideAggregatedMicrodata {
            get { return this.Retrieve(x => x.HideAggregatedMicrodata, false); }
            set { this.Store(x => x.HideAggregatedMicrodata, value); }
        }

        public string CanonicalUrl {
            get { return this.Retrieve(x => x.CanonicalUrl); }
            set { this.Store(x => x.CanonicalUrl, value); }
        }
    }
}