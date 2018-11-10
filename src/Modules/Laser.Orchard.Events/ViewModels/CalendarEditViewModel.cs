using Orchard.Data.Conventions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Events.ViewModels
{
    public class CalendarEditViewModel
    {
        /// <summary>
        /// Query/Layout ID
        /// </summary>
        [Required(ErrorMessage = "You must select a Query and a Layout")]
        public string QueryLayoutRecordId { get; set; }

        /// <summary>
        /// List of query records
        /// </summary>
        public IEnumerable<QueryRecordEntry> QueryRecordEntries { get; set; }

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

    public class QueryRecordEntry {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<LayoutRecordEntry> LayoutRecordEntries { get; set; }
    }

    public class LayoutRecordEntry {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}