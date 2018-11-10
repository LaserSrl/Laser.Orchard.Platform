using Laser.Orchard.Events.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Data.Conventions;
using Orchard.Projections.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Events.Models
{
    /// <summary>
    /// The content part model, uses the record class for storage.
    /// </summary>
    public class CalendarPart : ContentPart<CalendarPartRecord>
    {
        internal LazyField<List<AggregatedEventViewModel>> _eventList = new LazyField<List<AggregatedEventViewModel>>();

        /// <summary>
        /// The query to execute
        /// </summary>
        [Aggregate]
        public QueryPartRecord QueryPartRecord
        {
            get { return Record.QueryPartRecord; }
            set { Record.QueryPartRecord = value; }
        }

        /// <summary>
        /// The layout to render
        /// </summary>
        [Aggregate]
        public LayoutRecord LayoutRecord
        {
            get { return Record.LayoutRecord; }
            set { Record.LayoutRecord = value; }
        }

        /// <summary>
        /// Shape used to display the calendar
        /// </summary>
        public string CalendarShape
        {
            get { return this.Retrieve(r => r.CalendarShape); }
            set { this.Store(r => r.CalendarShape, value); }
        }

        /// <summary>
        /// Number of items per page in the list shape
        /// </summary>
        public int ItemsPerPage
        {
            get { return this.Retrieve(r => r.ItemsPerPage); }
            set { this.Store(r => r.ItemsPerPage, value); }
        }

        /// <summary>
        /// Gets or sets a flag telling whether to display the pager in the list shape
        /// </summary>
        public bool DisplayPager
        {
            get { return this.Retrieve(r => r.DisplayPager); }
            set { this.Store(r => r.DisplayPager, value); }
        }

        /// <summary>
        /// Suffix to use when multiple pagers are available on the same page
        /// </summary>
        [StringLength(255)]
        public string PagerSuffix
        {
            get { return this.Retrieve(r => r.PagerSuffix); }
            set { this.Store(r => r.PagerSuffix, value); }
        }

        /// <summary>
        /// Starting date of the list, as a tokenized field
        /// </summary>
        public string StartDate
        {
            get { return this.Retrieve(r => r.StartDate); }
            set { this.Store(r => r.StartDate, value); }
        }

        /// <summary>
        /// Number of days displayed in the list, as a tokenized field
        /// </summary>
        public string NumDays
        {
            get { return this.Retrieve(r => r.NumDays); }
            set { this.Store(r => r.NumDays, value); }
        }

        /// <summary>
        /// Aggregated list of events
        /// </summary>
        public List<AggregatedEventViewModel> Events
        {
            get { return _eventList.Value; }
        }
    }
}