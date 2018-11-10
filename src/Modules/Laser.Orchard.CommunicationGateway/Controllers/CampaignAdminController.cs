using Laser.Orchard.CommunicationGateway.Helpers;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.Controllers {

    public class CampaignAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "CommunicationCampaign";
        private readonly dynamic TestPermission = Permissions.ManageCampaigns;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public CampaignAdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                ((dynamic)newContent).CommunicationCampaignPart.FromDate.DateTime = DateTime.Now;
            //    ((dynamic)newContent).CommunicationCampaignPart.ToDate.DateTime = DateTime.Now.AddYears(1);
                model = _contentManager.BuildEditor(newContent);
            } else
                model = _contentManager.BuildEditor(_orchardServices.ContentManager.Get(id));
            return View((object)model);
        }

        [HttpPost, ActionName("Edit"), Admin, ValidateInput(false)]
        public ActionResult EditPOST(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            ContentItem content;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent, VersionOptions.Draft);
                content = newContent;
            } else
                content = _orchardServices.ContentManager.Get(id, VersionOptions.DraftRequired);
            var model = _orchardServices.ContentManager.UpdateEditor(content, this);

            if (!ModelState.IsValid) {
                foreach (string key in ModelState.Keys) {
                    if (ModelState[key].Errors.Count > 0)
                        foreach (var error in ModelState[key].Errors)
                            _notifier.Add(NotifyType.Error, T(error.ErrorMessage));
                }
                _orchardServices.TransactionManager.Cancel();
                return View(model);
            }
            _contentManager.Publish(content);
            _notifier.Add(NotifyType.Information, T("Campaign saved"));
            return RedirectToAction("Edit",new { id = content.Id });
        }

        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _orchardServices.ContentManager.Get(id);
            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType("CommunicationAdvertising");

            IEnumerable<ContentItem> ListContent = contentQuery.List().Where(x => (((dynamic)x).CommunicationAdvertisingPart.CampaignId.Equals(id)));
            if (ListContent.Count() == 0)
                _orchardServices.ContentManager.Remove(content);
            else
                _notifier.Add(NotifyType.Warning, T("Can't remove campaign with advertise"));

            return RedirectToAction("Index", "CampaignAdmin");
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, SearchVM search, bool ShowVideo = false) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search, ShowVideo);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, SearchVM search, bool ShowVideo = false) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            dynamic Options = new System.Dynamic.ExpandoObject();
            Options.ShowVideo = false;
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            var expression = search.Expression;
            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType(contentType).OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);
            //IEnumerable<ContentItem> ListContent = contentQuery.List();
            if (contentQuery.Count() == 0 || ShowVideo) {
                if (_orchardServices.ContentManager.Query<TitlePart, TitlePartRecord>("Video").Where(x => x.Title == "HowTo" + contentType).List().Count() > 0) {
                    Options.ShowVideo = true;
                    Options.VideoContent = _orchardServices.ContentManager.Query<TitlePart, TitlePartRecord>("Video").Where(x => x.Title == "HowTo" + contentType).List().Where(y => y.ContentItem.IsPublished()).FirstOrDefault().ContentItem;
                }
            }
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