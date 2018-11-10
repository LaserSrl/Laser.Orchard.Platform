using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Core.Common.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.SEO.ViewModels {
    public class SeoPartViewModel {
        [MaxLength(255)]
        public string TitleOverride { get; set; }

        [MaxLength(400)]
        public string CanonicalUrl { get; set; }
        [MaxLength(255)]
        public string Keywords { get; set; }
        [MaxLength(400)]
        public string Description { get; set; }      
        public bool RobotsNoIndex { get; set; }
        public bool RobotsNoFollow { get; set; }
        public bool RobotsNoSnippet { get; set; }
        public bool RobotsNoOdp { get; set; }
        public bool RobotsNoArchive { get; set; }
        public bool RobotsUnavailableAfter { get; set; }
        private DateTime? _robotsUnavailableAfterDate;
        public DateTimeEditor RobotsUnavailableAfterDateEditor {
            get {
                return new DateTimeEditor {
                    Date = _robotsUnavailableAfterDate == null
                        ? _seoServices.DateToString(DateTime.MinValue)
                        : _seoServices.DateToString(_robotsUnavailableAfterDate.Value),
                    Time = "",
                    ShowDate = true,
                    ShowTime = false
                };
            }
            set {
                _robotsUnavailableAfterDate = _seoServices.LocalDateFromString(value.Date);
            }
        }
        public bool RobotsNoImageIndex { get; set; }
        public bool GoogleNoSiteLinkSearchBox { get; set; }
        public bool GoogleNoTranslate { get; set; }
        public bool HasDetailMicrodata { get; set; }
        public bool HasAggregatedMicrodata { get; set; }
        public bool HideDetailMicrodata { get; set; }
        public bool HideAggregatedMicrodata { get; set; }

        //injected services
        private readonly ISEOServices _seoServices;

        /// <summary>
        /// default empty constructor
        /// </summary>
        public SeoPartViewModel(ISEOServices seoServices) {
            _seoServices = seoServices;
        }

        /// <summary>
        /// Create the ViewModel from the Part. 
        /// </summary>
        /// <param name="part">The SeoPart we start from.</param>
        /// <param name="seoServices">Dependency injection for services.</param>
        public SeoPartViewModel(SeoPart part, ISEOServices seoServices) : this(seoServices) {

            this.TitleOverride = part.TitleOverride;
            this.CanonicalUrl = part.CanonicalUrl;
            this.Keywords = part.Keywords;
            this.Description = part.Description;
            this.RobotsNoIndex = part.RobotsNoIndex;
            this.RobotsNoFollow = part.RobotsNoFollow;
            this.RobotsNoSnippet = part.RobotsNoSnippet;
            this.RobotsNoOdp = part.RobotsNoOdp;
            this.RobotsNoArchive = part.RobotsNoArchive;
            this.RobotsUnavailableAfter = part.RobotsUnavailableAfter;
            this._robotsUnavailableAfterDate = _seoServices.DateToLocal(part.RobotsUnavailableAfterDate);
            //this.RobotsUnavailableAfterDate = seoServices.DateToLocal(part.RobotsUnavailableAfterDate).ToShortDateString();
            this.RobotsNoImageIndex = part.RobotsNoImageIndex;
            this.GoogleNoSiteLinkSearchBox = part.GoogleNoSiteLinkSearchBox;
            this.GoogleNoTranslate = part.GoogleNoTranslate;
            this.HideDetailMicrodata = part.HideDetailMicrodata;
            this.HideAggregatedMicrodata = part.HideAggregatedMicrodata;
            this.HasDetailMicrodata = !string.IsNullOrWhiteSpace(part.Settings.GetModel<SeoPartSettings>().JsonLd);
            this.HasAggregatedMicrodata = part.Settings.GetModel<SeoPartSettings>().ShowAggregatedMicrodata;
        }

        /// <summary>
        /// Update a part based on the values of properties in the view model.
        /// </summary>
        /// <param name="part">The SeoPart we are going to update.</param>
        /// <param name="seoServices">Dependency injection for services.</param>
        public void UpdatePart(SeoPart part) {
            part.TitleOverride = this.TitleOverride;
            part.CanonicalUrl= this.CanonicalUrl;
            part.Keywords = this.Keywords;
            part.Description = this.Description;
            part.RobotsNoIndex = this.RobotsNoIndex;
            part.RobotsNoFollow = this.RobotsNoFollow;
            part.RobotsNoSnippet = this.RobotsNoSnippet;
            part.RobotsNoOdp = this.RobotsNoOdp;
            part.RobotsNoArchive = this.RobotsNoArchive;
            part.RobotsUnavailableAfter = this.RobotsUnavailableAfter;
            part.RobotsUnavailableAfterDate = _seoServices.DateToUTC(this._robotsUnavailableAfterDate);
            //part.RobotsUnavailableAfterDate = seoServices.DateToUTC(this.RobotsUnavailableAfterDate);
            part.RobotsNoImageIndex = this.RobotsNoImageIndex;
            part.GoogleNoSiteLinkSearchBox = this.GoogleNoSiteLinkSearchBox;
            part.GoogleNoTranslate = this.GoogleNoTranslate;
            part.HideDetailMicrodata = this.HideDetailMicrodata;
            part.HideAggregatedMicrodata = this.HideAggregatedMicrodata;
        }
    }
}