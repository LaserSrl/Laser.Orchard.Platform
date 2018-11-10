using Orchard.ContentManagement.Records;
using System;

namespace Laser.Orchard.Events.Models
{
    public class ActivityPartRecord : ContentPartRecord
    {
        public virtual DateTime? DateTimeStart { get; set; }
        public virtual DateTime? DateTimeEnd { get; set; }
        public virtual bool AllDay { get; set; }
        public virtual bool Repeat { get; set; }
        public virtual string RepeatType { get; set; }
        public virtual int RepeatValue { get; set; }
        public virtual string RepeatDetails { get; set; }
        public virtual bool RepeatEnd { get; set; }
        public virtual DateTime? RepeatEndDate { get; set; }
    }
}