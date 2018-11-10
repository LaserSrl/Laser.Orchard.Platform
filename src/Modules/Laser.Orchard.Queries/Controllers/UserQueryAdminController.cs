using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.Queries.Controllers {

    public class prebuilder {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class UserQueryAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IQueryService _queryService;
        private readonly IProjectionManager _projectionManager;

        private readonly string contentType = "Query";
        private readonly dynamic TestPermission = Permissions.UserQuery;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public UserQueryAdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager,
            IQueryService queryService,
             IProjectionManager projectionManager,
             IShapeFactory shapeFactory) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _queryService = queryService;
            _projectionManager = projectionManager;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }

        [Admin]
        public ActionResult Create() {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            var model = new prebuilder();
            model.Id = 0;
            model.Title = "";
            return View("Create", (object)model);
        }

        [Admin]
        public ActionResult CreateOneShot() {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            var model = new prebuilder();
            model.Id = 0;
            model.Title = "";
            return View((object)model);
        }

        [HttpPost, Admin]
        public ActionResult Create(prebuilder model) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            if (!string.IsNullOrEmpty(model.Title)) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent);
                newContent.As<TitlePart>().Title = model.Title;
                newContent.As<QueryPart>().Name = model.Title;
                //var userquery = (BooleanField)
                //          ((IEnumerable<ContentPart>)newContent.Parts)
                //          .SelectMany(p => p.Fields)
                //          .FirstOrDefault(f => f.Name == "UserQuery");
                //userquery.Value = true;
                ((dynamic)newContent).QueryUserFilterExtensionPart.UserQuery.Value = true;
                newContent.As<TitlePart>().Title = model.Title;
                return RedirectToAction("Index", "MyQueryAdmin");
                //  return RedirectToAction("Edit", "Filter", new {newContent.Id });
            }
            else
                return RedirectToAction("Index", "MyQueryAdmin");
        }

        [HttpPost, Admin]
        public ActionResult CreateOneShot(prebuilder model) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            if (!string.IsNullOrEmpty(model.Title)) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent);
                newContent.As<TitlePart>().Title = model.Title;
                newContent.As<QueryPart>().Name = model.Title;
                ((dynamic)newContent).QueryUserFilterExtensionPart.UserQuery.Value = true;
                ((dynamic)newContent).QueryUserFilterExtensionPart.OneShotQuery.Value = true;
                newContent.As<TitlePart>().Title = model.Title;
                return RedirectToAction("Index", "MyQueryAdmin");
            }
            else {
                return RedirectToAction("Index", "MyQueryAdmin");
            }
        }

        public ActionResult Delete(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission, T("Not authorized to manage queries")))
                return new HttpUnauthorizedResult();

            var query = _queryService.GetQuery(id);

            if (query == null) {
                return HttpNotFound();
            }

            _orchardServices.ContentManager.Remove(query.ContentItem);
            _orchardServices.Notifier.Information(T("Query {0} deleted", query.Name));

            return RedirectToAction("Index", "MyQueryAdmin");
        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission, T("Not authorized to edit queries")))
                return new HttpUnauthorizedResult();

            var query = _queryService.GetQuery(id);
            var viewModel = new AdminEditViewModel {
                Id = query.Id,
                Name = query.Name
            };

            #region Load Filters

            var filterGroupEntries = new List<FilterGroupEntry>();
            var allFilters = _projectionManager.DescribeFilters().SelectMany(x => x.Descriptors).ToList();

            foreach (var group in query.FilterGroups) {
                var filterEntries = new List<FilterEntry>();

                foreach (var filter in group.Filters) {
                    var category = filter.Category;
                    var type = filter.Type;

                    var f = allFilters.FirstOrDefault(x => category == x.Category && type == x.Type);
                    if (f != null) {
                        filterEntries.Add(
                            new FilterEntry {
                                Category = f.Category,
                                Type = f.Type,
                                FilterRecordId = filter.Id,
                                DisplayText = String.IsNullOrWhiteSpace(filter.Description) ? f.Display(new FilterContext { State = FormParametersHelper.ToDynamic(filter.State) }).Text : filter.Description
                            });
                    }
                }

                filterGroupEntries.Add(new FilterGroupEntry { Id = group.Id, Filters = filterEntries });
            }

            viewModel.FilterGroups = filterGroupEntries;

            #endregion Load Filters

            #region Load Sort criterias

            var sortCriterionEntries = new List<SortCriterionEntry>();
            var allSortCriteria = _projectionManager.DescribeSortCriteria().SelectMany(x => x.Descriptors).ToList();

            foreach (var sortCriterion in query.SortCriteria.OrderBy(s => s.Position)) {
                var category = sortCriterion.Category;
                var type = sortCriterion.Type;

                var f = allSortCriteria.FirstOrDefault(x => category == x.Category && type == x.Type);
                if (f != null) {
                    sortCriterionEntries.Add(
                        new SortCriterionEntry {
                            Category = f.Category,
                            Type = f.Type,
                            SortCriterionRecordId = sortCriterion.Id,
                            DisplayText = String.IsNullOrWhiteSpace(sortCriterion.Description) ? f.Display(new SortCriterionContext { State = FormParametersHelper.ToDynamic(sortCriterion.State) }).Text : sortCriterion.Description
                        });
                }
            }

            viewModel.SortCriteria = sortCriterionEntries;

            #endregion Load Sort criterias

            #region Load Layouts

            var layoutEntries = new List<LayoutEntry>();
            var allLayouts = _projectionManager.DescribeLayouts().SelectMany(x => x.Descriptors).ToList();

            foreach (var layout in query.Layouts) {
                var category = layout.Category;
                var type = layout.Type;

                var f = allLayouts.FirstOrDefault(x => category == x.Category && type == x.Type);
                if (f != null) {
                    layoutEntries.Add(
                        new LayoutEntry {
                            Category = f.Category,
                            Type = f.Type,
                            LayoutRecordId = layout.Id,
                            DisplayText = String.IsNullOrWhiteSpace(layout.Description) ? f.Display(new LayoutContext { State = FormParametersHelper.ToDynamic(layout.State) }).Text : layout.Description
                        });
                }
            }

            viewModel.Layouts = layoutEntries;

            #endregion Load Layouts

            return View(viewModel);
        }

        [Admin]
        public ActionResult Preview(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            var contentItems = _projectionManager.GetContentItems(id, 0, 20);
            var contentShapes = contentItems.Select(item => _orchardServices.ContentManager.BuildDisplay(item, "Summary"));

            var list = Shape.List();
            list.AddRange(contentShapes);

            return View(list);
        }

        //[Admin]
        //public ActionResult Edit(int id) {
        //    if (!_orchardServices.Authorizer.Authorize(TestPermission))
        //        return new HttpUnauthorizedResult();
        //    object model;
        //    if (id == 0) {
        //        var newContent = _orchardServices.ContentManager.New(contentType);
        //        //  model = _orchardServices.ContentManager.BuildEditor(newContent);
        //        //   _contentManager.Create(newContent);
        //        model = _contentManager.BuildEditor(newContent);
        //    }
        //    else
        //        model = _contentManager.BuildEditor(_orchardServices.ContentManager.Get(id));
        //    return View((object)model);
        //}

        //[HttpPost, ActionName("Edit"), Admin]
        //public ActionResult EditPOST(int id) {
        //    if (!_orchardServices.Authorizer.Authorize(TestPermission))
        //        return new HttpUnauthorizedResult();

        //    ContentItem content;
        //    if (id == 0) {
        //        var newContent = _orchardServices.ContentManager.New(contentType);
        //        _orchardServices.ContentManager.Create(newContent);
        //        content = newContent;
        //    }
        //    else
        //        content = _orchardServices.ContentManager.Get(id);
        //    var model = _orchardServices.ContentManager.UpdateEditor(content, this);

        //    if (!ModelState.IsValid) {
        //        foreach (string key in ModelState.Keys) {
        //            if (ModelState[key].Errors.Count > 0)
        //                foreach (var error in ModelState[key].Errors)
        //                    _notifier.Add(NotifyType.Error, T(error.ErrorMessage));
        //        }
        //        _orchardServices.TransactionManager.Cancel();
        //        return View(model);
        //    }
        //    _notifier.Add(NotifyType.Information, T("Query saved"));
        //    return RedirectToAction("Index", "MyQueryAdmin");
        //}
        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}