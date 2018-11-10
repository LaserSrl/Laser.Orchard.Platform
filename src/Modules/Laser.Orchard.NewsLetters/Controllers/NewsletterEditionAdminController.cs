using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.NewsLetters.Models;
using Laser.Orchard.NewsLetters.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using Laser.Orchard.NewsLetters.Extensions;
//using Orchard.Core.Contents.Controllers;
using Orchard.UI.Notify;
using Orchard.DisplayManagement;
using Orchard.UI.Navigation;
using Orchard.Mvc;
using Orchard.Tasks.Scheduling;

namespace Laser.Orchard.NewsLetters.Controllers {
    [Admin]
    public class NewsletterEditionAdminController : Controller, IUpdateModel {
        private readonly INewsletterServices _newslServices;
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;
        private readonly IScheduledTaskManager _taskManager;

        public NewsletterEditionAdminController(IOrchardServices services,
                        ITransactionManager transactionManager,
                        IContentManager contentManager,
                        IShapeFactory shapeFactory,
                        INewsletterServices newsletterServices, 
                        IScheduledTaskManager taskManager) {
            _newslServices = newsletterServices;
            _transactionManager = transactionManager;
            _contentManager = contentManager;
            _taskManager = taskManager;
            Services = services;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;

        }
        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }




        //
        // GET: /NewsletterEditionAdmin/
        public ActionResult Index(int newsletterId, int? page, int? pageSize) {
            int currentPage = page.HasValue ? page.Value : 1;
            int defaultPageSize = pageSize.HasValue ? pageSize.Value : 10;
            var pagerParameters = new PagerParameters {
                Page = page,
                PageSize = pageSize
            };

            var list = Services.New.List();
            var editions =_newslServices.GetNewsletterEditions(newsletterId, VersionOptions.Latest).OrderBy(o=>o.Number.HasValue).ThenByDescending(o=>o.Number); 
            list.AddRange(editions
                .Skip((currentPage - 1) * defaultPageSize)
                .Take(defaultPageSize)
                .Select(b => {
                    var newsletter = Services.ContentManager.BuildDisplay(b, "SummaryAdmin");
                    return newsletter;
                }));
            Pager pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);
            dynamic pagerShape = Services.New.Pager(pager).TotalItemCount(editions.Count());

            dynamic viewModel = Services.New.ViewModel()
                .ContentItems(list)
                .NewsletterId(newsletterId)
                .Pager(pagerShape);


            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);

        }
        #region [ Create ]
        public ActionResult Create(int newsletterId) {
            //if (!Services.Authorizer.Authorize(Permissions.ManageNewsletterEditions, T("Not allowed to create newsletters")))
            //    return new HttpUnauthorizedResult();

            NewsletterEditionPart newsletterEdition = Services.ContentManager.New<NewsletterEditionPart>("NewsletterEdition");

            if (newsletterEdition == null)
                return HttpNotFound();

            dynamic model = Services.ContentManager.BuildEditor(newsletterEdition);


            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        [ValidateInput(false)]
        [FormValueRequired("submit.Save")]
        public ActionResult CreatePOST(int newsletterId) {
            //if (!Services.Authorizer.Authorize(Permissions.ManageNewsletterDefinitions, T("Couldn't create newsletter")))
            //    return new HttpUnauthorizedResult();
            var newsletterEdition = Services.ContentManager.New<NewsletterEditionPart>("NewsletterEdition");

            var returnResult = EditionSave(ref newsletterEdition);
            if (returnResult == null) {
                return Redirect(Url.NewsLetterEditionEdit(newsletterId, newsletterEdition));
            } else {
                return returnResult;
            }
        }

        [HttpPost, ActionName("Create")]
        [ValidateInput(false)]
        [FormValueRequired("submit.SendNewsletter")]
        public ActionResult CreateAndSend(int newsletterId) {
            //if (!Services.Authorizer.Authorize(Permissions.ManageNewsletterDefinitions, T("Couldn't create newsletter")))
            //    return new HttpUnauthorizedResult();
            var newsletterEdition = Services.ContentManager.New<NewsletterEditionPart>("NewsletterEdition");

            var returnResult = EditionSave(ref newsletterEdition);
            if (returnResult == null) {
                if (Request.Form["submit.SendNewsletter"] == "submit.SendNewsletterTest") {
                    //_newslServices.SendNewsletterEdition(ref newsletterEdition, true, Request.Form["Newsletters.SendNewsletterEmailTest"]);
                    _newslServices.SendNewsletterEdition(ref newsletterEdition, Request.Form["Newsletters.SendNewsletterEmailTest"]);
                } else {
                    //_newslServices.SendNewsletterEdition(ref newsletterEdition);
                    ContentItem newsletter = newsletterEdition.ContentItem;
                    _taskManager.CreateTask("Laser.Orchard.NewsLetters.SendEdition.Task", DateTime.UtcNow.AddMinutes(1), newsletter);
                }

                return Redirect(Url.NewsLetterEditionEdit(newsletterId, newsletterEdition));
            } else {
                return returnResult;
            }
        }

        #endregion

        #region [ Edit ]
        public ActionResult Edit(int Id, int newsletterId) {
            var newsletter = _newslServices.GetNewsletterEdition(Id, VersionOptions.Latest);

            //if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, newsletter, T("Not allowed to edit newsletter")))
            //    return new HttpUnauthorizedResult();

            if (newsletter == null)
                return HttpNotFound();

            dynamic model = Services.ContentManager.BuildEditor(newsletter);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateInput(false)]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditDeletePOST(int Id, int newsletterId) {
            //if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't delete newsletter")))
            //    return new HttpUnauthorizedResult();

            var newsletter = _newslServices.GetNewsletterEdition(Id, VersionOptions.DraftRequired);
            if (newsletter == null)
                return HttpNotFound();
            _newslServices.DeleteNewsletterEdition(newsletter);

            Services.Notifier.Information(T("Newsletter edition deleted"));

            return Redirect(Url.NewsLettersForAdmin());
        }


        [HttpPost, ActionName("Edit")]
        [ValidateInput(false)]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int Id, int newsletterId) {
            var newsletter = _newslServices.GetNewsletterEdition(Id, VersionOptions.DraftRequired);

            //if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, newsletter, T("Couldn't edit newsletter")))
            //    return new HttpUnauthorizedResult();

            if (newsletter == null)
                return HttpNotFound();

            dynamic model = Services.ContentManager.UpdateEditor(newsletter, this);
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            _contentManager.Publish(newsletter);
            Services.Notifier.Information(T("Newsletter edition updated"));

            return Redirect(Url.NewsLetterEditionEdit(newsletterId, newsletter.As<NewsletterEditionPart>()));
        }

        [HttpPost, ActionName("Edit")]
        [ValidateInput(false)]
        [FormValueRequired("submit.SendNewsletter")]
        public ActionResult EditAndSend(int Id, int newsletterId) {
            ContentItem newsletter = _newslServices.GetNewsletterEdition(Id, VersionOptions.DraftRequired);

            //if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, newsletter, T("Couldn't edit newsletter")))
            //    return new HttpUnauthorizedResult();

            if (newsletter == null)
                return HttpNotFound();

            dynamic model = Services.ContentManager.UpdateEditor(newsletter, this);
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Newsletter edition updated"));
            var newsletterEditionpart = newsletter.As<NewsletterEditionPart>();
            if (Request.Form["submit.SendNewsletter"] == "submit.SendNewsletterTest") {
                //_newslServices.SendNewsletterEdition(ref newsletterEditionpart, true, Request.Form["Newsletters.SendNewsletterEmailTest"]);
                _newslServices.SendNewsletterEdition(ref newsletterEditionpart, Request.Form["Newsletters.SendNewsletterEmailTest"]);
                _contentManager.Publish(newsletter);
            } else {
                //_newslServices.SendNewsletterEdition(ref newsletterEditionpart);
                _taskManager.CreateTask("Laser.Orchard.NewsLetters.SendEdition.Task", DateTime.UtcNow.AddMinutes(1), newsletter);
            }
            
            return Redirect(Url.NewsLetterEditionEdit(newsletterId, newsletter.As<NewsletterEditionPart>()));
        }

        #endregion

        #region [ private ]
        private ActionResult EditionSave(ref NewsletterEditionPart newsletterEdition) {

            _contentManager.Create(newsletterEdition, VersionOptions.Draft);
            dynamic model = _contentManager.UpdateEditor(newsletterEdition, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.ContentManager.Publish(newsletterEdition.ContentItem);
            return null;

        }

        #endregion

        #region [ IUpdateModel Implementation ]
        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        #endregion
    }
}
