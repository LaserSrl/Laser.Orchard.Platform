using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Projections.Models;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Laser.Orchard.CommunicationGateway.Controllers {

    [ValidateInput(false)]
    public class AdvertisingAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "CommunicationAdvertising";
        private readonly dynamic TestPermission = Permissions.ManageCommunicationAdv;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public AdvertisingAdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        [Admin]
        public ActionResult Edit(int id, int idCampaign = 0) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _contentManager.New(contentType);
                if (idCampaign > 0) {
                    newContent.As<CommunicationAdvertisingPart>().CampaignId = idCampaign;
                }
                //  model = _contentManager.BuildEditor(newContent);
                //   _contentManager.Create(newContent);
                model = _contentManager.BuildEditor(newContent);
            }
            else
                model = _contentManager.BuildEditor(_contentManager.Get(id, VersionOptions.Latest));
            return View((object)model);
        }

        [HttpPost, ActionName("Edit"), Admin]
        public ActionResult EditPOST(int id, int idCampaign = 0) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            ContentItem content;
            if (id == 0) {
                var newContent = _contentManager.New(contentType);
                _contentManager.Create(newContent, VersionOptions.Draft);
                content = newContent;
            }
            else
                content = _contentManager.Get(id, VersionOptions.DraftRequired);
            content.As<CommunicationAdvertisingPart>().CampaignId = idCampaign > 0 ? idCampaign : 0;
            if (idCampaign > 0) {
                dynamic campaignContent = _contentManager.Get(idCampaign);

                // Controllo validità della campagna
                if (campaignContent.CommunicationCampaignPart.ToDate.DateTime != null && campaignContent.CommunicationCampaignPart.ToDate.DateTime != DateTime.MinValue && campaignContent.CommunicationCampaignPart.ToDate.DateTime <= DateTime.UtcNow) {
                    _notifier.Add(NotifyType.Error, T("Campaign validity has expired. No changes allowed."));
                    return RedirectToAction("Index", "AdvertisingAdmin", new { id = idCampaign });
                }
            }

            var model = _contentManager.UpdateEditor(content, this);
            if (!ModelState.IsValid) {
                foreach (string key in ModelState.Keys) {
                    if (ModelState[key].Errors.Count > 0)
                        foreach (var error in ModelState[key].Errors)
                            _notifier.Add(NotifyType.Error, T(error.ErrorMessage));
                }
                _orchardServices.TransactionManager.Cancel();
                return View(model);
            }
            _notifier.Add(NotifyType.Information, T("Advertising saved"));
            if (Request.Form["submit.Publish"] == "submit.Publish") {
                // _contentManager.Unpublish(content);
                _contentManager.Publish(content);
                //  _contentManager.Unpublish(content); // inserito per permettere il publishlater
            }
            return RedirectToAction("Edit", new { id = content.Id, idCampaign = idCampaign });
        }

        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id, int idCampaign = 0) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _contentManager.Get(id, VersionOptions.DraftRequired);
            //ContentItem content;// = _contentManager.Get(id, VersionOptions.DraftRequired);
            //List<ContentItem> li = _orchardServices.ContentManager.GetAllVersions(id).ToList();
            //      if (li.Count() == 1)
            //          content = li[0];
            //    else
            //          content = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
           
            _contentManager.Remove(content);
            return RedirectToAction("Index", "AdvertisingAdmin", new { id = idCampaign });
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int id, int? page, int? pageSize, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search, id);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, SearchVM search, int id = 0) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            dynamic Options = new System.Dynamic.ExpandoObject();
            if (id >= 0)
                Options.Campaign = _contentManager.Get(id);
            else {
                // Options.Campaign = ""; // devo inserire la proprietà Campaign altrimenti index va in exception
                Options.Campaign = new System.Dynamic.ExpandoObject();
                Options.Campaign.Id = id;
            }
            var expression = search.Expression;
            IContentQuery<ContentItem> contentQuery = _contentManager.Query(VersionOptions.Latest).ForType(contentType).OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);
            /*Nel caso di flash advertising la campagna è -10, quindi il filtro è sempre valido.*/
            if (id > 0)
                contentQuery = contentQuery.Where<CommunicationAdvertisingPartRecord>(w =>
                    w.CampaignId.Equals(id)
                    );
            else
                contentQuery = contentQuery.Join<CommunicationAdvertisingPartRecord>().Where(w =>
                    w.CampaignId.Equals(0));
            if (!string.IsNullOrEmpty(search.Expression))
                contentQuery = contentQuery.Where<TitlePartRecord>(w => w.Title.Contains(expression));
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(contentQuery.Count());
            var pageOfContentItems = contentQuery.Slice(pager.GetStartIndex(), pager.PageSize)
                .Select(p => new ContentIndexVM {
                    Id = p.Id,
                    Title = ((dynamic)p).TitlePart.Title,
                    ModifiedUtc = ((dynamic)p).CommonPart.ModifiedUtc,
                    UserName = ((dynamic)p).CommonPart.Owner != null ? ((dynamic)p).CommonPart.Owner.UserName : "Anonymous",
                    ContentItem = p
                }).ToList();
            var model = new SearchIndexVM(pageOfContentItems, search, pagerShape, Options);
            return View((object)model);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}