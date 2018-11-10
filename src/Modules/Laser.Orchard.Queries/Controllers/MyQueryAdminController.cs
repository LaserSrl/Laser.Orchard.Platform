using Laser.Orchard.Queries.Helpers;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.Queries.ViewModels;
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

namespace Laser.Orchard.Queries.Controllers {

    public class MyQueryAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly ICustomQuery _customQuery;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "MyCustomQuery";
        private readonly dynamic TestPermission = Permissions.UserQuery;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public MyQueryAdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager,
            ICustomQuery customQuery) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            _customQuery = customQuery;
            T = NullLocalizer.Instance;
        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                //  model = _orchardServices.ContentManager.BuildEditor(newContent);
                //   _contentManager.Create(newContent);
                model = _contentManager.BuildEditor(newContent);
            }
            else
                model = _contentManager.BuildEditor(_orchardServices.ContentManager.Get(id));
            return View((object)model);
        }

        [HttpPost, ActionName("Edit"), Admin]
        public ActionResult EditPOST(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            ContentItem content;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent);
                content = newContent;
            }
            else
                content = _orchardServices.ContentManager.Get(id);
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
            _notifier.Add(NotifyType.Information, T("Query saved"));
            return RedirectToAction("Index", "MyQueryAdmin");
        }

        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _orchardServices.ContentManager.Get(id);
            _orchardServices.ContentManager.Remove(content);
            _notifier.Add(NotifyType.Warning, T("Query Removed"));
            return RedirectToAction("Index", "MyQueryAdmin");
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, SearchVM search) {
            dynamic Options = new System.Dynamic.ExpandoObject();
            Options.ShowVideo = false;
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            var expression = search.Expression;
            string[] listcontentstype = new string[] { contentType, "Query" };

            //IEnumerable<ContentItem> ListContent1 = _orchardServices.ContentManager.Query(VersionOptions.Latest).ForType(listcontentstype).List().Where(x => x.ContentType == contentType || ((dynamic)x).QueryUserFilterExtensionPart.UserQuery.Value == true);

            //  IEnumerable<ContentItem> ListContent=  _orchardServices.ContentManager.Query(VersionOptions.Latest).ForType(listcontentstype).OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc).List().Where(x => x.ContentType == contentType || ((dynamic)x).QueryUserFilterExtensionPart.UserQuery.Value == true);
            IEnumerable<ContentItem> ListContent = _customQuery.ListContent();
            if (!string.IsNullOrEmpty(search.Expression))
                ListContent = from content in ListContent
                              where
                              ((content.As<TitlePart>().Title ?? "").Contains(expression, StringComparison.InvariantCultureIgnoreCase))
                              select content;
            IEnumerable<ContentIndexVM> listVM = ListContent.Select(p => new ContentIndexVM {
                Id = p.Id,
                Title = p.As<TitlePart>().Title,
                ModifiedUtc = p.As<CommonPart>().ModifiedUtc,
                UserName = p.As<CommonPart>().Owner.UserName,
                ContentType = p.ContentType,
                OneShotQuery = ( p.Parts.FirstOrDefault(x => x.PartDefinition.Name == "QueryUserFilterExtensionPart") != null) ? ((dynamic)p).QueryUserFilterExtensionPart.OneShotQuery.Value ?? false : false
            });

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(listVM.Count());
            var list = listVM.Skip(pager.GetStartIndex())
                                .Take(pager.PageSize);
            //_orchardServices.New.List();
            //list.AddRange(listVM.Skip(pager.GetStartIndex())
            //                    .Take(pager.PageSize)
            //                    );
            var model = new SearchIndexVM(list, search, pagerShape, Options);
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