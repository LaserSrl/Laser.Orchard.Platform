using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Projections.Models;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Events.Models
{
    public class CalendarPartRecord : ContentPartRecord
    {
        /// <summary>
        /// The query to execute
        /// </summary>
        [Aggregate]
        public virtual QueryPartRecord QueryPartRecord { get; set; }

        /// <summary>
        /// The layout to render
        /// </summary>
        [Aggregate]
        public virtual LayoutRecord LayoutRecord { get; set; }

        /// <summary>
        /// Shape used to display the calendar
        /// </summary>
        public virtual string CalendarShape { get; set; }

        /// <summary>
        /// Number of items per page in the list shape
        /// </summary>
        public virtual int ItemsPerPage { get; set; }

        /// <summary>
        /// True to render a pager in the list shape
        /// </summary>
        public virtual bool DisplayPager { get; set; }

        /// <summary>
        /// Suffix to use when multiple pagers are available on the same page
        /// </summary>
        [StringLength(255)]
        public virtual string PagerSuffix { get; set; }

        /// <summary>
        /// Starting date of the list, as a tokenized field
        /// </summary>
        public virtual string StartDate { get; set; }

        /// <summary>
        /// Number of days displayed in the list, as a tokenized field
        /// </summary>
        public virtual string NumDays { get; set; }
    }
}