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
using Orchard.Mvc;

namespace Laser.Orchard.NewsLetters.Controllers {
    [Admin]
    public class NewsletterAdminController : Controller, IUpdateModel {
        private readonly INewsletterServices _newslServices;
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;

        public NewsletterAdminController(IOrchardServices services,
                        ITransactionManager transactionManager,
                        IContentManager contentManager,
                        INewsletterServices newsletterDefinitionServices) {
            _newslServices = newsletterDefinitionServices;
            _transactionManager = transactionManager;
            _contentManager = contentManager;
            Services = services;
            T = NullLocalizer.Instance;

        }
        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        //
        // GET: /NewsletterAdmin/
        public ActionResult Index() {

            var list = Services.New.List();
            list.AddRange(_newslServices.GetNewsletterDefinition(VersionOptions.Latest)
                .Select(b => {
                    var newsletter = Services.ContentManager.BuildDisplay(b, "SummaryAdmin");
                    return newsletter;
                }));

            dynamic viewModel = Services.New.ViewModel()
                .ContentItems(list);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);

        }
        #region [ Create ]
        public ActionResult Create() {
            //if (!Services.Authorizer.Authorize(Permissions.ManageNewsletterDefinitions, T("Not allowed to create newsletters")))
            //    return new HttpUnauthorizedResult();

            NewsletterDefinitionPart newsletterDefinition = Services.ContentManager.New<NewsletterDefinitionPart>("NewsletterDefinition");
            if (newsletterDefinition == null)
                return HttpNotFound();

            dynamic model = Services.ContentManager.BuildEditor(newsletterDefinition);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        [ValidateInput(false)]
        public ActionResult CreatePOST() {
            //if (!Services.Authorizer.Authorize(Permissions.ManageNewsletterDefinitions, T("Couldn't create newsletter")))
            //    return new HttpUnauthorizedResult();

            var newsletterDefinition = Services.ContentManager.New<NewsletterDefinitionPart>("NewsletterDefinition");

            _contentManager.Create(newsletterDefinition, VersionOptions.Draft);
            dynamic model = _contentManager.UpdateEditor(newsletterDefinition, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.ContentManager.Publish(newsletterDefinition.ContentItem);
            return Redirect(Url.NewsLetterEdit(newsletterDefinition));
        }
        #endregion
        
        #region [ Edit ]
        public ActionResult Edit(int newsletterId) {
            var newsletter = _newslServices.GetNewsletterDefinition(newsletterId, VersionOptions.Latest);

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
        public ActionResult EditDeletePOST(int newsletterId) {
            //if (!Services.Authorizer.Authorize(Permissions.ManageBlogs, T("Couldn't delete newsletter")))
            //    return new HttpUnauthorizedResult();

            var newsletter = _newslServices.GetNewsletterDefinition(newsletterId, VersionOptions.DraftRequired);
            if (newsletter == null)
                return HttpNotFound();
            _newslServices.DeleteNewsletterDefinition(newsletter);

            Services.Notifier.Information(T("Newsletter deleted"));

            return Redirect(Url.NewsLettersForAdmin());
        }


        [HttpPost, ActionName("Edit")]
        [ValidateInput(false)]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int newsletterId) {
            var newsletter = _newslServices.GetNewsletterDefinition(newsletterId, VersionOptions.DraftRequired);

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

            return Redirect(Url.NewsLetterEdit(newsletter.As<NewsletterDefinitionPart>()));
        }

        #endregion

        #region [ Delete ]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Remove(int newsletterId) {

            var newsl = _newslServices.GetNewsletterDefinition(newsletterId, VersionOptions.Latest);

            if (newsl == null)
                return HttpNotFound();

            _newslServices.DeleteNewsletterDefinition(newsl);

            Services.Notifier.Information(T("newsletter was successfully deleted"));
            return Redirect(Url.NewsLettersForAdmin());
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
