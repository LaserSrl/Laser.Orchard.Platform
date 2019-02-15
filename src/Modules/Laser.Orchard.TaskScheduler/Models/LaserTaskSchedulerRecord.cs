using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.Models {
    public class LaserTaskSchedulerRecord : ContentPartRecord {

        public virtual string SignalName { get; set; }
        public virtual DateTime? ScheduledStartUTC { get; set; }
        public virtual int PeriodicityTime { get; set; }
        public virtual string PeriodicityUnit { get; set; }
        public virtual int ContentItemId { get; set; }
        public virtual int RunningTaskId { get; set; }
        public virtual bool Autodestroy { get; set; }
        public virtual string ExecutionType { get; set; }
        public virtual bool LongTask { get; set; }

    }
}