using Laser.Orchard.TaskScheduler.Models;
using Laser.Orchard.TaskScheduler.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Scheduling.Models;
using Orchard.Data;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Orchard.Localization.Services;
using System.Globalization;
using Laser.Orchard.StartupConfig.Localization;

namespace Laser.Orchard.TaskScheduler.Services {

    public class ScheduledTaskService : IScheduledTaskService {
        private readonly IRepository<LaserTaskSchedulerRecord> _repoLaserTaskScheduler;
        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IRepository<ScheduledTaskRecord> _repoTasks;
        private readonly IDateLocalizationServices _dateServices;
        private readonly IDateLocalization _dateLocalization;
        

        public ScheduledTaskService(IRepository<LaserTaskSchedulerRecord> repoLaserTaskScheduler,
            IOrchardServices orchardServices,
            IScheduledTaskManager taskManager,
            IRepository<ScheduledTaskRecord> repoTasks,
            IDateLocalizationServices dateServices,
            IDateLocalization dateLocalization) {
            _repoLaserTaskScheduler = repoLaserTaskScheduler;
            _orchardServices = orchardServices;
            _taskManager = taskManager;
            _repoTasks = repoTasks;
            _dateServices = dateServices;
            _dateLocalization = dateLocalization;
        }

        /// <summary>
        /// Get all the scheulers from the db
        /// </summary>
        /// <returns>A list of task schedulers found in the db</returns>
        public List<ScheduledTaskPart> GetAllTasks() {
            List<ScheduledTaskPart> parts = _orchardServices.ContentManager.Query("ScheduledTask").ForPart<ScheduledTaskPart>().List().ToList();
            foreach (ScheduledTaskPart pa in parts.Where(p => p.RunningTaskId != 0)) {
                //check whether the task is still running. It might have been stopped by someone or something
                if (_repoTasks.Get(pa.RunningTaskId) == null) {
                    pa.RunningTaskId = 0;
                }
            }
            return parts;
        }

        /// <summary>
        /// Converts the data from the form into a list of view models for the task schedulers
        /// NOTE: here we are using the name of the properties and fields in the strings:
        /// if those are changed for any reason, the strings in this method should reflect that
        /// </summary>
        /// <param name="formData">The collection representing the data from the form</param>
        /// <returns>The list of view models</returns>
        public List<ScheduledTaskViewModel> GetTaskViewModelsFromForm(NameValueCollection formData) {
            var keys = formData.AllKeys.Where(k => k.IndexOf("allTasks[") == 0).ToArray();
            List<ScheduledTaskViewModel> vmsForTasks = new List<ScheduledTaskViewModel>();
            int nVms = keys.Where(x => x.EndsWith("].Id")).Count();
            for (int i = 0; i < nVms; i++) {
                string kk = (keys.Where(x => x.EndsWith("].Id"))).ToArray()[i];
            
                int index = int.Parse(kk.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                string thisObject = String.Format("allTasks[{0}].", index);
                DateTime? inputDate;
                try {
                    string formDate = formData[thisObject + "ScheduledStartUTCEditor.Date"];
                    string formTime = formData[thisObject + "ScheduledStartUTCEditor.Time"];
                    //inputDate = _dateServices.ConvertFromLocalizedString(formDate, formTime);
                    inputDate = _dateLocalization.StringToDatetime(formDate, formTime);
                }
                catch (Exception) {
                    inputDate = null;
                }
                vmsForTasks.Add(new ScheduledTaskViewModel(_orchardServices, _dateServices) {
                    Id = int.Parse(formData[thisObject + "Id"]),
                    SignalName = formData[thisObject + "SignalName"],
                    ScheduledStartUTC = inputDate,
                    PeriodicityTime = int.Parse(formData[thisObject + "PeriodicityTime"]),
                    ExecutionType = EnumExtension<ExecutionTypes>.ParseEnum(formData[thisObject + "ExecutionType"]),
                    PeriodicityUnit = EnumExtension<TimeUnits>.ParseEnum(formData[thisObject + "PeriodicityUnit"]),
                    ContentItemId = int.Parse(formData[thisObject + "ContentItemId"]),
                    Running = int.Parse(formData[thisObject + "Running"]),
                    Delete = Convert.ToBoolean(formData[thisObject + "Delete"]),
                    Scheduling = Convert.ToBoolean(formData[thisObject + "Scheduling"]),
                    Autodestroy= (formData[thisObject + "Autodestroy"]).Contains("true"),
                    LongTask = (formData[thisObject + "LongTask"]).Contains("true")
                });
            }

            return vmsForTasks;
        }

        /// <summary>
        /// Updates the reords for the schedulers based on the changes from the UI
        /// </summary>
        /// <param name="vms">A list of view models that hold the updated information</param>
        public void UpdateRecords(List<ScheduledTaskViewModel> vms) {
            foreach (ScheduledTaskViewModel vm in vms) {
                //if Id != 0 the task was already in the db
                if (vm.Id != 0) {
                    //Should we try to delete?
                    if (vm.Delete) {
                        //if there is a corresponding task that is running, we should stop it first
                        if (vm.Running > 0) {
                            //stop the task with id == vm.Running
                            UnscheduleTask(vm);
                        }
                        //the task is definitely not running, so we may safely remove the scheduler
                        _orchardServices.ContentManager.Remove(_orchardServices.ContentManager.Get(vm.Id));
                        //(note that a handler is invoked to clean up the repositor)
                    }
                    else {
                        //update the part
                        ScheduledTaskPart part = (ScheduledTaskPart)_orchardServices.ContentManager.Get<ScheduledTaskPart>(vm.Id);
                        vm.UpdatePart(part);
                    }
                }
                else {
                    //we have to create a new record
                    if (!vm.Delete) {
                        //we only create it if it was not also deleted already
                        ScheduledTaskPart part = (ScheduledTaskPart)_orchardServices.ContentManager.New<ScheduledTaskPart>("ScheduledTask");
                        vm.UpdatePart(part);
                        _orchardServices.ContentManager.Create(part);
                        vm.Id = part.Id;
                    }
                }
            }
        }

        /// <summary>
        /// Schedule a new task based on the information in the view model
        /// </summary>
        /// <param name="vm">The view model we are basing the new task on</param>
        public void ScheduleTask(ScheduledTaskViewModel vm) {
            //get the part
            ScheduledTaskPart part = (ScheduledTaskPart)_orchardServices.ContentManager.Get<ScheduledTaskPart>(vm.Id);
            //define tasktype: BASE_SIGNALNAME_ID
            string taskTypeStr = Constants.TaskTypeBase + "_" + part.SignalName + "_" + part.Id;
            ContentItem ci = null;
            if (part.ContentItemId > 0) {
                ci = _orchardServices.ContentManager.Get(part.ContentItemId);
            }
            _taskManager.CreateTask(taskTypeStr, part.ScheduledStartUTC ?? DateTime.UtcNow, ci);
            part.RunningTaskId = _repoTasks.Get(str => str.TaskType.Equals(taskTypeStr)).Id;
        }

        public void UpdateRecordsAndSchedule(List<ScheduledTaskViewModel> vms) {
            foreach (ScheduledTaskViewModel vm in vms) {
                //if Id != 0 the task was already in the db
                if (vm.Id != 0) {
                    //Should we try to delete?
                    if (vm.Delete) {
                        //if there is a corresponding task that is running, we should stop it first
                        if (vm.Running > 0) {
                            //stop the task with id == vm.Running
                            UnscheduleTask(vm);
                        }
                        //the task is definitely not running, so we may safely remove the scheduler
                        _orchardServices.ContentManager.Remove(_orchardServices.ContentManager.Get(vm.Id));
                        //(note that a handler is invoked to clean up the repositor)
                    } else {
                        //update the part
                        ScheduledTaskPart part = (ScheduledTaskPart)_orchardServices.ContentManager.Get<ScheduledTaskPart>(vm.Id);
                        vm.UpdatePart(part);
                    }
                } else {
                    //we have to create a new record
                    if (!vm.Delete) {
                        //we only create it if it was not also deleted already
                        ScheduledTaskPart part = (ScheduledTaskPart)_orchardServices.ContentManager.New<ScheduledTaskPart>("ScheduledTask");
                        vm.UpdatePart(part);
                        _orchardServices.ContentManager.Create(part);
                        vm.Id = part.Id;

                        string taskTypeStr = Constants.TaskTypeBase + "_" + part.SignalName + "_" + part.Id;
                        if (vm.LinkedContent==null && part.ContentItemId > 0) {
                            vm.LinkedContent = _orchardServices.ContentManager.Get(part.ContentItemId);
                        }
                        part.RunningTaskId = CreateTask(taskTypeStr, part.ScheduledStartUTC ?? DateTime.UtcNow,vm.LinkedContent);
                    }
                }
            }
        }

        private int CreateTask(string action, DateTime scheduledUtc, ContentItem contentItem) {
            var taskRecord = new ScheduledTaskRecord {
                TaskType = action,
                ScheduledUtc = scheduledUtc,
            };
            if (contentItem != null) {
                taskRecord.ContentItemVersionRecord = contentItem.VersionRecord;
            }
            _repoTasks.Create(taskRecord);
            return taskRecord.Id;
        }




        /// <summary>
        /// Unschedule an existing task based on the view model
        /// </summary>
        /// <param name="vm">The view model corresponding to the task we want to unschedule</param>
        public void UnscheduleTask(ScheduledTaskViewModel vm) {
            //get the part
            ScheduledTaskPart part = (ScheduledTaskPart)_orchardServices.ContentManager.Get<ScheduledTaskPart>(vm.Id);
            int tId = part.RunningTaskId;
            if (tId > 0) {
                var str = _repoTasks.Get(tId);
                if (str != null) {
                    _repoTasks.Delete(str);
                }
                else {
                    //tId might have changed since the moment we got the information into the view models
                    //e.g. if the task is periodic, it will generate a new Id and update it.
                    //let's check here if there are tasks with the part id in the TaskType
                    //(see the ScheduleTask method for the format we are using)
                    var records = _repoTasks.Table.ToList().Where(rec =>
                        //rec.TaskType.Split(new string[]{"_"}, StringSplitOptions.RemoveEmptyEntries).Last().Equals(part.Id.ToString())
                        rec.TaskType.IndexOf(Constants.TaskTypeBase) == 0
                        ).ToList().Where(rec =>
                        rec.TaskType.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries).Last().Equals(part.Id.ToString())
                        ).ToList();
                    foreach (var item in records) {
                        _repoTasks.Delete(item);
                    }
                }
            }

            part.RunningTaskId = 0;
        }

        /// <summary>
        /// computes the next DateTime for scheduling based off the information in the part
        /// </summary>
        /// <param name="part">The part containing the scheduling information</param>
        /// <returns>A <type>DateTime</type> object containing the moment when the task whoudl be scheduled next.</returns>
        public DateTime ComputeNextScheduledTime(ScheduledTaskPart part) {
            DateTime result = part.ScheduledStartUTC == null ? DateTime.UtcNow : part.ScheduledStartUTC.Value;
            // incrementa la start date in base alla periodicità fino a raggiungere una start date futura
            // la periodicità è sicuramente > 0 come verificato nell'handler, quindi il ciclo seguente non è infinito
            while (result <= DateTime.UtcNow) {
                switch (part.PeriodicityUnit) {
                    case TimeUnits.Seconds:
                        result = result.AddSeconds(part.PeriodicityTime);
                        break;

                    case TimeUnits.Minutes:
                        result = result.AddMinutes(part.PeriodicityTime);
                        break;

                    case TimeUnits.Hours:
                        result = result.AddHours(part.PeriodicityTime);
                        break;

                    case TimeUnits.Days:
                        result = result.AddDays(part.PeriodicityTime);
                        break;

                    case TimeUnits.Weeks:
                        result = result.AddDays(7 * part.PeriodicityTime);
                        break;

                    case TimeUnits.Months:
                        result = result.AddMonths(part.PeriodicityTime);
                        break;

                    case TimeUnits.Years:
                        result = result.AddYears(part.PeriodicityTime);
                        break;

                    default:
                        break;
                }
            }
            part.ScheduledStartUTC = result;
            return result;
        }
    }
}