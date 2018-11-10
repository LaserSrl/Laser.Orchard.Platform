using Laser.Orchard.TaskScheduler.Services;
using Laser.Orchard.TaskScheduler.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using OMvc = Orchard.Mvc;
using Orchard.Localization.Services;

namespace Laser.Orchard.TaskScheduler.Controllers {
    public class AdminController : Controller {

        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskService _scheduledTaskService;
        private readonly IDateLocalizationServices _dateServices;

        public Localizer T { get; set; }

        public AdminController(IOrchardServices orchardServices, IScheduledTaskService scheduledTaskService, IDateLocalizationServices dateServices) {
            _orchardServices = orchardServices;
            _scheduledTaskService = scheduledTaskService;
            _dateServices = dateServices;

            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to schedule periodic tasks")))
                return new HttpUnauthorizedResult();

            IndexViewModel vm = new IndexViewModel(_scheduledTaskService.GetAllTasks(), _orchardServices, _dateServices);

            return View(vm);
        }

        [HttpPost, ActionName("Index")]
        [OMvc.FormValueRequired("submit.SaveSchedulers")]
        public ActionResult IndexSaveSchedulers(IndexViewModel ivm) {
            var formData = ControllerContext.RequestContext.HttpContext.Request.Form;
            List<ScheduledTaskViewModel> vmsForTasks = _scheduledTaskService.GetTaskViewModelsFromForm(formData);
            _scheduledTaskService.UpdateRecords(vmsForTasks);

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Index")]
        [OMvc.FormValueRequired("submit.UnscheduleTask")]
        public ActionResult IndexUnscheduleTask(IndexViewModel ivm) {
            var formData = ControllerContext.RequestContext.HttpContext.Request.Form;
            List<ScheduledTaskViewModel> vmsForTasks = _scheduledTaskService.GetTaskViewModelsFromForm(formData);
            _scheduledTaskService.UpdateRecords(vmsForTasks);
            //get the vm for the task we are trying to unschedule
            ScheduledTaskViewModel vmToUnschedule = vmsForTasks.Where(vm => vm.Scheduling).FirstOrDefault();
            if (vmToUnschedule != null) {
                _scheduledTaskService.UnscheduleTask(vmToUnschedule); ;
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Index")]
        [OMvc.FormValueRequired("submit.ScheduleTask")]
        public ActionResult IndexScheduleTask(IndexViewModel ivm) {
            var formData = ControllerContext.RequestContext.HttpContext.Request.Form;
            List<ScheduledTaskViewModel> vmsForTasks = _scheduledTaskService.GetTaskViewModelsFromForm(formData);
            _scheduledTaskService.UpdateRecords(vmsForTasks);
            //get the vm for the task we are trying to schedule
            ScheduledTaskViewModel vmToSchedule = vmsForTasks.Where(vm => vm.Scheduling).FirstOrDefault();
            if (vmToSchedule != null) {
                _scheduledTaskService.ScheduleTask(vmToSchedule);
            }

            return RedirectToAction("Index");
        }
    }
}