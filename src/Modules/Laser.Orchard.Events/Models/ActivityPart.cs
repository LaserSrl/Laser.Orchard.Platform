using Orchard.ContentManagement;
using System;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Events.Models
{
    /// <summary>
    /// The content part model, uses the record class for storage.
    /// </summary>
    public class ActivityPart : ContentPart<ActivityPartRecord>
    {
        /// <summary>
        /// Gets or sets the activity start date and time
        /// </summary>
        [DataType(DataType.DateTime)]
        [Required]
        [Range(typeof(DateTime?), "1-Jan-1900", "1-Jan-2060")]
        public DateTime? DateTimeStart
        {
            get { return this.Retrieve(r => r.DateTimeStart); }
            set { this.Store(r => r.DateTimeStart, value); }
        }

        /// <summary>
        /// Gets or sets the activity end date and time
        /// </summary>
        [DataType(DataType.DateTime)]
        [Required]
        [Range(typeof(DateTime?), "1-Jan-1900", "1-Jan-2060")]
        public DateTime? DateTimeEnd
        {
            get { return this.Retrieve(r => r.DateTimeEnd); }
            set { this.Store(r => r.DateTimeEnd, value); }
        }

        /// <summary>
        /// Gets or sets a flag telling whether the activity must be associated with the entire day or not
        /// </summary>
        public bool AllDay
        {
            get { return this.Retrieve(r => r.AllDay); }
            set { this.Store(r => r.AllDay, value); }
        }

        /// <summary>
        /// Gets or sets a flag telling whether the activity must be repeated
        /// </summary>
        public bool Repeat
        {
            get { return this.Retrieve(r => r.Repeat); }
            set { this.Store(r => r.Repeat, value); }
        }

        /// <summary>
        /// Gets or sets the periodicity type of the repeated activity (i.e. daily, monthly, etc.)
        /// </summary>
        public string RepeatType
        {
            get { return this.Retrieve(r => r.RepeatType); }
            set { this.Store(r => r.RepeatType, value); }
        }

        /// <summary>
        /// Gets or sets the periodicity value of the repeated activity (i.e. if RepeatType is 'M' and RepeatValue is '2' it means 'every two months')
        /// </summary>
        [Required]
        public int RepeatValue
        {
            get { return this.Retrieve(r => r.RepeatValue); }
            set { this.Store(r => r.RepeatValue, value); }
        }

        /// <summary>
        /// Gets or sets the details of the repeated activity (i.e. specific days when it must be repeated)
        /// </summary>
        public string RepeatDetails
        {
            get { return this.Retrieve(r => r.RepeatDetails); }
            set { this.Store(r => r.RepeatDetails, value); }
        }

        /// <summary>
        /// Gets or sets a flag telling if the repeated activity must end at a specific date
        /// </summary>
        public bool RepeatEnd
        {
            get { return this.Retrieve(r => r.RepeatEnd); }
            set { this.Store(r => r.RepeatEnd, value); }
        }

        /// <summary>
        /// Gets or sets a flag telling the date when the repeating activity must stop
        /// </summary>
        [DataType(DataType.Date)]
        [Required]
        [Range(typeof(DateTime?), "1-Jan-1900", "1-Jan-2060")]
        public DateTime? RepeatEndDate
        {
            get { return this.Retrieve(r => r.RepeatEndDate); }
            set { this.Store(r => r.RepeatEndDate, value); }
        }
    }
}