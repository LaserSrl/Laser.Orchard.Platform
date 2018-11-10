using Laser.Orchard.Vimeo.Extensions;
using Laser.Orchard.Vimeo.Services;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Handlers {
    public class VimeoScheduledTasksHandler : IScheduledTaskHandler {
        private readonly IScheduledTaskManager _taskManager;
        private readonly IVimeoTaskServices _vimeoTaskServices;

        public VimeoScheduledTasksHandler(IScheduledTaskManager taskManager, IVimeoTaskServices vimeoTaskServices) {
            _taskManager = taskManager;
            _vimeoTaskServices = vimeoTaskServices;
        }

        public void Process(ScheduledTaskContext context){
            string taskTypeStr = context.Task.TaskType;
            if (taskTypeStr == Constants.TaskTypeBase + Constants.TaskSubtypeInProgress) {
                //call service to verify the state of the uploads
                if (_vimeoTaskServices.VerifyAllUploads() > 0) {
                    //there is still stuff in the repository, so we should reschedule the task
                    _vimeoTaskServices.ScheduleUploadVerification();
                }
            } else if (taskTypeStr == Constants.TaskTypeBase + Constants.TaskSubtypeComplete) {
                //call service to verify status of completed uploads
                if (_vimeoTaskServices.TerminateUploads() > 0) {
                    //there is still stuff in the repository, so we should reschedule the task
                    _vimeoTaskServices.ScheduleVideoCompletion();
                }
            }
        }
    }
}