using System;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.SEO.Models {
    #region Obsolete code
    //[Obsolete("Replaced with 'SeoVersionRecord' to enable versioning of this content")]
    //public class SeoRecord : ContentPartRecord {
    //    public virtual string TitleOverride { get; set; }
    //    public virtual string Keywords { get; set; }
    //    public virtual string Description { get; set; }
    //}
    #endregion

    public class SeoVersionRecord : ContentPartVersionRecord {
        public virtual string TitleOverride { get; set; }
        public virtual string Keywords { get; set; }
        public virtual string Description { get; set; }
        //20160620: update SEOPart based off supported metadata: https://support.google.com/webmasters/answer/79812?hl=en
        public virtual bool RobotsNoIndex { get; set; }
        public virtual bool RobotsNoFollow { get; set; }
        public virtual bool RobotsNoSnippet { get; set; }
        public virtual bool RobotsNoOdp { get; set; }
        public virtual bool RobotsNoArchive { get; set; }
        public virtual bool RobotsUnavailableAfter { get; set; }
        public virtual DateTime? RobotsUnavailableAfterDate { get; set; }
        public virtual bool RobotsNoImageIndex { get; set; }
        public virtual bool GoogleNoSiteLinkSearchBox { get; set; }
        public virtual bool GoogleNoTranslate { get; set; }
        public virtual bool HideDetailMicrodata { get; set; }
        public virtual bool HideAggregatedMicrodata { get; set; }
        public virtual string CanonicalUrl { get; set; }
    }

}