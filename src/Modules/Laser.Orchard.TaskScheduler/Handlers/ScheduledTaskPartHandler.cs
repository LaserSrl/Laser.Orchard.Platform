using Laser.Orchard.TaskScheduler.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Scheduling.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.Handlers {
    public class ScheduledTaskPartHandler : ContentHandler {

        public ScheduledTaskPartHandler(IRepository<LaserTaskSchedulerRecord> repoTasks) {
            Filters.Add(StorageFilter.For(repoTasks));
            Filters.Add(new ActivatingFilter<ScheduledTaskPart>("ScheduledTask"));
        }
    }
}