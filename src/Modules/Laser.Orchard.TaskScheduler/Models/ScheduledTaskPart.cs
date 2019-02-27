using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.Models {
    public class ScheduledTaskPart : ContentPart<LaserTaskSchedulerRecord> {
        /// <summary>
        /// task will trigger this signal
        /// </summary>
        public string SignalName {
            get { return this.Retrieve(x => x.SignalName); }
            set { this.Store(x => x.SignalName, value); }
        }
        /// <summary>
        /// used if we want to create a task but not have it start right now
        /// </summary>
        public DateTime? ScheduledStartUTC {
            get { return this.Retrieve(x => x.ScheduledStartUTC); }
            set { this.Store(x => x.ScheduledStartUTC, value); }
        }
        /// <summary>
        /// 0 for no repeat
        /// </summary>
        public int PeriodicityTime {
            get { return this.Retrieve(x => x.PeriodicityTime); }
            set { this.Store(x => x.PeriodicityTime, value); }
        }
        /// <summary>
        /// use to reschedule periodic task
        /// </summary>
        public TimeUnits PeriodicityUnit {
            get { return EnumExtension<TimeUnits>.ParseEnum(this.Retrieve(x => x.PeriodicityUnit)); }
            set { this.Store(x => x.PeriodicityUnit, value.ToString()); }
        }
        /// <summary>
        /// will pass the corresponding item when triggering the signal
        /// </summary>
        public int ContentItemId {
            get { return this.Retrieve(x => x.ContentItemId); }
            set { this.Store(x => x.ContentItemId, value); }
        }
        /// <summary>
        /// Id of the running task corresponding to this scheduling
        /// </summary>
        public int RunningTaskId {
            get { return this.Retrieve(x => x.RunningTaskId); }
            set { this.Store(x => x.RunningTaskId, value); }
        }

        public bool Autodestroy {
            get { return this.Retrieve(x => x.Autodestroy); }
            set { this.Store(x => x.Autodestroy, value); }
        }
        public ExecutionTypes ExecutionType {
            get { return EnumExtension<ExecutionTypes>.ParseEnum(this.Retrieve(x => x.ExecutionType)); }
            set { this.Store(x => x.ExecutionType, value.ToString()); }
        }
        public bool LongTask {
            get { return this.Retrieve(x => x.LongTask); }
            set { this.Store(x => x.LongTask, value); }
        }
    }
}