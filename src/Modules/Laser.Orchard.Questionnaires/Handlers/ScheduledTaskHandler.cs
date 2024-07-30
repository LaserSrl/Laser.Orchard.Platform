using Laser.Orchard.Questionnaires.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Linq;

namespace Laser.Orchard.Questionnaires.Handlers {

    public class ScheduledTaskHandler : IScheduledTaskHandler {
        public const string TaskType = "QuestionnaireRanking"; //made this public rather than private
        private readonly IScheduledTaskManager _taskManager;
        private readonly IQuestionnairesServices _questionnairesServices;

        public ILogger Logger { get; set; }


        /*
         * as this is now, the task does not get eliminated after being completed becacuse we recreate it
         * */
        public ScheduledTaskHandler(IScheduledTaskManager taskManager, IQuestionnairesServices questionnairesServices) {
            _questionnairesServices = questionnairesServices;
            _taskManager = taskManager;
            Logger = NullLogger.Instance;
            try {
                //DateTime firstDate = DateTime.UtcNow.AddHours(6);//DateTime.UtcNow.AddSeconds(30);//new DateTime().AddMinutes(5);
                //ScheduleNextTask(firstDate);
            } catch (Exception e) {
                this.Logger.Error(e, e.Message);
            }
        }

        public void Process(ScheduledTaskContext context) {
            try {
                //  this.Logger.Error("sono dentro process");
                string taskTypeStr = context.Task.TaskType;
                string[] taTypeParts = taskTypeStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (taTypeParts.Length == 2) {
                    if (taTypeParts[0] == TaskType) {
                        bool sent = false;
                        try {
                            int gId = int.Parse(taTypeParts[1]);
                            sent = _questionnairesServices.SendTemplatedEmailRanking(gId);
                        } catch (Exception e) {
                            this.Logger.Error(e, e.Message);
                            throw;
                        }
                        if (!sent) {
                            //reschedule
                            DateTime nextDate = ((DateTime)(context.Task.ScheduledUtc)).AddMinutes(5);
                            _taskManager.CreateTask(taskTypeStr, nextDate, context.Task.ContentItem);
                        }
                    }
                }
            } catch (Exception ex) {
                string idcontenuto = "nessun id ";
                try {
                    idcontenuto = context.Task.ContentItem.Id.ToString();
                } catch (Exception ex2) { Logger.Error(ex2, ex2.Message); }
                Logger.Error(ex, "Error on " + TaskType + " analized input: " + context.Task.TaskType + " for ContentItem id = " + idcontenuto + " : " + ex.Message);
            }
            //if (context.Task.TaskType == TaskType) {
            //    try {

            //        bool sended= _questionnairesServices.SendTemplatedEmailRanking();
            //        //The following line does not work here, because the task does not contain the ContentItem
            //       //_questionnairesServices.SendTemplatedEmailRanking(context.Task.ContentItem.Id);
            //    }
            //    catch (Exception e) {
            //        this.Logger.Error(e, e.Message);
            //    }
            //    finally {
            //        DateTime nextTaskDate = DateTime.UtcNow.AddHours(6); //DateTime.UtcNow.AddSeconds(30);//
            //        ScheduleNextTask(nextTaskDate);
            //    }
            //}
        }

        private void ScheduleNextTask(DateTime date) {
            if (date > DateTime.UtcNow) {
                var tasks = this._taskManager.GetTasks(TaskType);
                if (tasks == null || tasks.Count() == 0) //this prevents from scheduling an email task if another email task is already scheduled
                    this._taskManager.CreateTask(TaskType, date, null);
            }
        }

    }
}