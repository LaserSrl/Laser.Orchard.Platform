using Laser.Orchard.TaskScheduler.Models;
using Laser.Orchard.TaskScheduler.ViewModels;
using Orchard;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.TaskScheduler.Services {
    public interface IScheduledTaskService : IDependency {

        List<ScheduledTaskPart> GetAllTasks();
        List<ScheduledTaskViewModel> GetTaskViewModelsFromForm(NameValueCollection formData);
        void UpdateRecords(List<ScheduledTaskViewModel> vms);
        void ScheduleTask(ScheduledTaskViewModel vm);
        void UpdateRecordsAndSchedule(List<ScheduledTaskViewModel> vms);
        void UnscheduleTask(ScheduledTaskViewModel vm);
        DateTime ComputeNextScheduledTime(ScheduledTaskPart part);
    }
}
