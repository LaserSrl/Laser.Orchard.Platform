using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Laser.Orchard.GoogleAnalytics.Models {
    public class GoogleAnalyticsSettingsPart : ContentPart<GoogleAnalyticsSettingsPartRecord> {

        /// <summary>
        /// Gets or sets the Google Analytics tracking key used to perform analytics tracking.
        /// </summary>
        [RegularExpression(@"(^UA\-\d{1,}\-\d{1,}$)|(^GTM\-\w+$)")]
        public string GoogleAnalyticsKey {
            get { return this.Retrieve(r => r.GoogleAnalyticsKey); }
            set { this.Store(r => r.GoogleAnalyticsKey, value); }
        }

        /// <summary>
        /// Gets or sets the override domain name that may optionally be used for performing things like multiple domain/sub-domain tracking.
        /// </summary>
        public string DomainName {
            get { return this.Retrieve(r => r.DomainName); }
            set { this.Store(r => r.DomainName, value); }
        }

        /// <summary>
        /// Gets or sets whether the new Google Analytics asynchronous tracking method should be used.
        /// </summary>
        public bool UseAsyncTracking {
            get { return this.Retrieve(r => r.UseAsyncTracking); }
            set { this.Store(r => r.UseAsyncTracking, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Google Analytics tracking will anonymize IP.
        /// </summary>
        public bool AnonymizeIp {
            get { return this.Retrieve(r => r.AnonymizeIp); }
            set { this.Store(r => r.AnonymizeIp, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Google Analytics tracking will be included on /Admin pages.
        /// </summary>
        public bool TrackOnAdmin {
            get { return this.Retrieve(r => r.TrackOnAdmin); }
            set { this.Store(r => r.TrackOnAdmin, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Google Analytics tracking will be included on front end pages.
        /// </summary>
        public bool TrackOnFrontEnd {
            get { return this.Retrieve(r => r.TrackOnFrontEnd); }
            set { this.Store(r => r.TrackOnFrontEnd, value); }
        }

        public bool UseTagManager {
            get { return this.Retrieve(r => r.UseTagManager); }
            set { this.Store(r => r.UseTagManager, value); }
        }
    }
}