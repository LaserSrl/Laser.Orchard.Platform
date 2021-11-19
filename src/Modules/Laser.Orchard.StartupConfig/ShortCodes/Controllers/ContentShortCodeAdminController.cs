using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Contents;
using Orchard.Core.Contents.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Themes;
using Orchard.UI.Notify;
using Laser.Orchard.StartupConfig.ShortCodes.Settings.Models;
using System.Web.Routing;
using Orchard.UI.Admin;

namespace Laser.Orchard.StartupConfig.ShortCodes.Controllers {
    [OrchardFeature("Laser.Orchard.ShortCodes")]
    public class ContentShortCodeAdminController : Controller {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INavigationManager _navigationManager;
        private readonly ISiteService _siteService;
        private readonly ICultureManager _cultureManager;
        private readonly ICultureFilter _cultureFilter;

        public ContentShortCodeAdminController(
                        IOrchardServices orchardServices,
                        IContentDefinitionManager contentDefinitionManager,
                        INavigationManager navigationManager,
                        ISiteService siteService,
                        ICultureManager cultureManager,
                        ICultureFilter cultureFilter) {
            Services = orchardServices;
            _contentDefinitionManager = contentDefinitionManager;
            _navigationManager = navigationManager;
            _siteService = siteService;
            _cultureManager = cultureManager;
            _cultureFilter = cultureFilter;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        public IOrchardServices Services { get; private set; }

        [Admin]
        public ActionResult Index(ListContentsViewModel model, PagerParameters pagerParameters, string part, string field, string type, int hostId) {
            // Check Permissions to Edit the content hosting shortcodes
            if (hostId > 0 && !Services.Authorizer.Authorize(Permissions.EditContent, Services.ContentManager.Get(hostId))) {
                Services.Notifier.Add(NotifyType.Error, T("Cannot add shortcode."));
                return new HttpUnauthorizedResult();
            }

            var menuItems = _navigationManager.BuildMenu("content-picker").ToList();
            var contentPickerMenuItem = menuItems.FirstOrDefault();
            if (contentPickerMenuItem == null) {
                return HttpNotFound();
            }

            if (contentPickerMenuItem.Items.All(x => x.Text.TextHint != "Recent Content")) {
                // the default tab should not be displayed, redirect to the next one
                var root = menuItems.FirstOrDefault();
                if (root == null) {
                    return HttpNotFound();
                }

                var firstChild = root.Items.First();
                if (firstChild == null) {
                    return HttpNotFound();
                }

                var routeData = new RouteValueDictionary(firstChild.RouteValues);
                var queryString = Request.QueryString;
                foreach (var key in queryString.AllKeys) {
                    if (!String.IsNullOrEmpty(key)) {
                        routeData[key] = queryString[key];
                    }
                }

                return RedirectToRoute(routeData);
            }

            // Filters the contents by ContentType or filters at least "Listable" content types
            ContentShortCodeSettings settings;
            var types = "";
            if (!string.IsNullOrWhiteSpace(part) && string.IsNullOrWhiteSpace(field)) {
                ContentTypePartDefinition defintion = _contentDefinitionManager.GetTypeDefinition(type).Parts.FirstOrDefault(x => x.PartDefinition.Name == part);
                if (defintion != null) {
                    settings = defintion.Settings.GetModel<ContentShortCodeSettings>();
                    types = settings.DisplayedContentTypes;
                }
            }
            else if (!string.IsNullOrWhiteSpace(part) && !string.IsNullOrWhiteSpace(field)) {
                ContentPartFieldDefinition defintion = _contentDefinitionManager.GetTypeDefinition(type)
                    .Parts.Where(x => x.PartDefinition.Name == part)
                    .SelectMany(x => x.PartDefinition.Fields)
                    .FirstOrDefault(x => x.Name == field);

                settings = defintion.Settings.GetModel<ContentShortCodeSettings>();
                types = settings.DisplayedContentTypes;
            }



            IEnumerable<ContentTypeDefinition> contentTypes;

            if (!String.IsNullOrEmpty(types)) {
                var rawTypes = types.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                contentTypes = _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(x => x.Parts.Any(p => rawTypes.Contains(p.PartDefinition.Name)) || rawTypes.Contains(x.Name))
                    .ToArray();
            }
            else {
                contentTypes = GetListableTypes(false).ToList();
            }

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var query = Services.ContentManager.Query(VersionOptions.Latest, contentTypes.Select(ctd => ctd.Name).ToArray());

            if (!string.IsNullOrEmpty(model.Options.SelectedFilter)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.Options.SelectedFilter);
                if (contentTypeDefinition == null)
                    return HttpNotFound();

                model.TypeDisplayName = !string.IsNullOrWhiteSpace(contentTypeDefinition.DisplayName)
                                            ? contentTypeDefinition.DisplayName
                                            : contentTypeDefinition.Name;
                query = query.ForType(model.Options.SelectedFilter);

            }

            switch (model.Options.OrderBy) {
                case ContentsOrder.Modified:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(model.Options.SelectedCulture)) {
                query = _cultureFilter.FilterCulture(query, model.Options.SelectedCulture);
            }

            model.Options.FilterOptions = contentTypes
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            model.Options.Cultures = _cultureManager.ListCultures();

            var pagerShape = Services.New.Pager(pager).TotalItemCount(query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            var list = Services.New.List();

            list.AddRange(pageOfContentItems.Select(ci => Services.ContentManager.BuildDisplay(ci, "SummaryAdmin")));

            foreach (IShape item in list.Items) {
                item.Metadata.Type = "ContentPicker";
            }

            var tab = Services.New.RecentContentTab()
                .ContentItems(list)
                .Pager(pagerShape)
                .Options(model.Options)
                .TypeDisplayName(model.TypeDisplayName ?? "");

            // retain the parameter in the pager links
            RouteData.Values["Options.SelectedFilter"] = model.Options.SelectedFilter;
            RouteData.Values["Options.OrderBy"] = model.Options.OrderBy.ToString();
            RouteData.Values["Options.ContentsStatus"] = model.Options.ContentsStatus.ToString();
            RouteData.Values["Options.SelectedCulture"] = model.Options.SelectedCulture;

            return new ShapeResult(this, Services.New.ShortCodes_ContentPicker().Tab(tab));
        }

        private IEnumerable<ContentTypeDefinition> GetListableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Listable && (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

    }
}