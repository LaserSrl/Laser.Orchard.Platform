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
using Orchard.Core.Contents.Controllers;
using Orchard.UI.Notify;
using Orchard.DisplayManagement;
using Orchard.UI.Navigation;
using OMvc = Orchard.Mvc;

namespace Laser.Orchard.NewsLetters.Controllers {
    [Admin]
    public class SubscribersAdminController : Controller, IUpdateModel {
        private readonly INewsletterServices _newslServices;
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;

        public SubscribersAdminController(IOrchardServices services,
                        ITransactionManager transactionManager,
                        IContentManager contentManager,
                        IShapeFactory shapeFactory,
                        INewsletterServices newsletterServices,
                        IOrchardServices orchardServices) {
            _newslServices = newsletterServices;
            _transactionManager = transactionManager;
            _contentManager = contentManager;
            Services = services;
            Shape = shapeFactory;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;

        }
        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }




        //
        // GET: /SubscribersAdmin/
        public ActionResult Index(int newsletterId, int? page, int? pageSize) {
            int currentPage = page.HasValue?page.Value:1;
            int defaultPageSize = pageSize.HasValue ? pageSize.Value : 10;
            var list = Services.New.List();

            var subscribers = _newslServices.GetSubscribers(newsletterId).OrderBy(o=>o.Name);
            var pagerParameters = new PagerParameters {
                Page = page,
                PageSize = pageSize
            };
            list.AddRange(subscribers
                .Skip((currentPage -1)* defaultPageSize)
                .Take(defaultPageSize));
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(subscribers.Count());

            dynamic viewModel = Services.New.ViewModel()
                .Subscribers(list)
                .NewsletterId(newsletterId)
                .Pager(pagerShape);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);

        }
        #region [ Create ]
        public ActionResult Create(int newsletterId) {
            //if (!Services.Authorizer.Authorize(Permissions.ManageSubscriberss, T("Not allowed to create newsletters")))
            //    return new HttpUnauthorizedResult();

            NewsletterEditionPart newsletterEdition = Services.ContentManager.New<NewsletterEditionPart>("NewsletterEdition");

            if (newsletterEdition == null)
                return HttpNotFound();

            dynamic model = Services.ContentManager.BuildEditor(newsletterEdition);


            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(int newsletterId) {
            //if (!Services.Authorizer.Authorize(Permissions.ManageNewsletterDefinitions, T("Couldn't create newsletter")))
            //    return new HttpUnauthorizedResult();

            var newsletter = Services.ContentManager.New<NewsletterEditionPart>("NewsletterEdition");

            _contentManager.Create(newsletter, VersionOptions.Draft);
            dynamic model = _contentManager.UpdateEditor(newsletter, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.ContentManager.Publish(newsletter.ContentItem);
            return Redirect(Url.NewsLetterEditionEdit(newsletterId, newsletter));
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
        [OMvc.FormValueRequired("submit.Delete")]
        public ActionResult EditDeletePOST(int Id, int newsletterId) {
            //if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't delete newsletter")))
            //    return new HttpUnauthorizedResult();

            var newsletter = _newslServices.GetNewsletterEdition(Id, VersionOptions.DraftRequired);
            if (newsletter == null)
                return HttpNotFound();
            _newslServices.DeleteNewsletterEdition(newsletter);

            Services.Notifier.Information(T("Newsletter deleted"));

            return Redirect(Url.NewsLettersForAdmin());
        }


        [HttpPost, ActionName("Edit")]
        [OMvc.FormValueRequired("submit.Save")]
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
            Services.Notifier.Information(T("Newsletter information updated"));

            return Redirect(Url.NewsLetterEditionEdit(newsletterId, newsletter.As<NewsletterEditionPart>()));
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
