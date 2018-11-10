using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Laser.Orchard.Commons.Services;
using Laser.Orchard.Highlights.Enums;
using Laser.Orchard.Highlights.Models;
using Laser.Orchard.Highlights.Settings;
using Laser.Orchard.Highlights.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.MediaLibrary.Fields;
using Orchard.Mvc.Html;
using Orchard.Themes.Models;
using Orchard.Themes.Services;

namespace Laser.Orchard.Highlights.Services {

    public class HighlightsService : IHighlightsService {


        private readonly IContentManager _contentManager;
        private readonly ICultureManager _cultureManager;
        private readonly IRepository<HighlightsItemPartRecord> _repository;
        private readonly LocalWebConfigManager _localConfiguration;
        private readonly IShapeFactory _shapeFactory;
        private readonly ISiteThemeService _siteThemeService;
        private readonly IOrchardServices _orchardServices;


        public HighlightsService(ISiteThemeService siteThemeService, IContentManager contentManager, IRepository<HighlightsItemPartRecord> repository,
            ICultureManager cultureManager, IShapeFactory shapeFactory, IOrchardServices orchardServices) {
            _contentManager = contentManager;
            _cultureManager = cultureManager;
            _repository = repository;
            _localConfiguration = new LocalWebConfigManager();
            _shapeFactory = shapeFactory;
            _siteThemeService = siteThemeService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;

        }

        /// <summary>
        /// Localizzazione
        /// </summary>
        public Localizer T {
            get;
            set;
        }

        public IEnumerable<HighlightsItemPart> GetHighlightsItemsByGroupId(int groupId) {
            var parts = _contentManager
              .Query<HighlightsItemPart, HighlightsItemPartRecord>(VersionOptions.Latest)
              .Where(e => e.HighlightsGroupPartRecordId == groupId)
              .OrderBy(o => o.ItemOrder);
            return (parts.List());
        }


        public IEnumerable<dynamic> GetHighlightsItemsContentByGroupId(int groupId, DisplayTemplate displayTemplate, string displayPlugin, string settingsShapeName) {

            //IContentQuery<HighlightsItemPart, LocalizationPartRecord> parts;

            var items = _contentManager
              .Query(VersionOptions.Published, "HighlightsItem")
              .Join<HighlightsItemPartRecord>()
              .Where<HighlightsItemPartRecord>(e => e.HighlightsGroupPartRecordId == groupId)
              .OrderBy(o => o.ItemOrder);


            var shapeList = new List<dynamic>();
            var i = 0;

            foreach (var item in items.List()) {
                var itemPart = item.As<HighlightsItemPart>();
                itemPart.GroupShapeName = settingsShapeName;
                itemPart.GroupDisplayTemplate = displayTemplate;
                itemPart.GroupDisplayPlugin = displayPlugin;
                var shape = _contentManager.BuildDisplay(item, displayTemplate.ToString());
                //shape.Index = i;
                //shape.ParentShapeName = settingsShapeName;
                shapeList.Add(shape);
                i++;
            }

            return (shapeList);
        }

        public HighlightsItemPart Get(int itemId) {
            return _contentManager.Get<HighlightsItemPart>(itemId);
        }

        public void UpdateOrder(int itemId, int order) {
            var item = _repository.Get(itemId);
            item.ItemOrder = order;
            _repository.Update(item);
        }

        public virtual void Remove(HighlightsItemPart mediaItem) {
            _contentManager.Remove(mediaItem.ContentItem);
        }



        public IList<string> GetAvailablePlugins() {
            var config = _localConfiguration.GetConfiguration("~/Modules/Laser.Orchard.Highlights");
            var list = config.AppSettings.Settings["available-plugins"]
                .Value.Split(',')
                .Select(s => s.Trim())
                .ToList();
            list.Insert(0, "");
            return list;
        }

        public ViewsInfos ChooseView(dynamic part) {
            if (part.GetType() == typeof(HighlightsGroupPart)) {
                var typeSettings = part.Settings.GetModel<HighlightsGroupSettings>();
                var defaultShapeName = "";
                var settingsShapeName = "";
                if (typeSettings != null && !String.IsNullOrEmpty(typeSettings.ShapeName)) {
                    settingsShapeName = "_" + typeSettings.ShapeName;
                }
                if (part.DisplayTemplate == Enums.DisplayTemplate.List) {
                    defaultShapeName = "Parts_Banner";
                    if (!settingsShapeName.StartsWith("_Banner")) {
                        settingsShapeName = "";
                    }
                } else if (part.DisplayTemplate == Enums.DisplayTemplate.SlideShow) {
                    defaultShapeName = "Parts_SlideShow";
                }
                // HS
                //if (part.DisplayPlugin != "" && part.DisplayPlugin != "(default)") {
                //    if (part.DisplayPlugin.StartsWith(part.DisplayTemplate.ToString() + " - ")) {
                //        var plugin = part.DisplayPlugin.Replace(part.DisplayTemplate.ToString() + " - ", "").Trim().Replace(" ","");
                //        if (plugin != "") {
                //            defaultShapeName += "_" + plugin;
                //        }
                //    }
                //}
                //TODO: Test if wiew exists in order to be sure to not have a 404
                return new ViewsInfos {
                    DefaultShapeName = defaultShapeName,
                    SuffixShapeName = settingsShapeName,
                    ResultShapeName = defaultShapeName + settingsShapeName
                };
            } else if (part.GetType() == typeof(HighlightsItemPart)) {
                var defaultShapeName = "Parts_HighlightsItem";
                var settingsShapeName = part.GroupShapeName != "" ? "_" + part.GroupShapeName : "";
                return new ViewsInfos {
                    DefaultShapeName = defaultShapeName,
                    SuffixShapeName = settingsShapeName,
                    ResultShapeName = defaultShapeName + settingsShapeName
                };
            } else if (part.GetType() == typeof(HighlightsItemViewModel)) {
                var defaultShapeName = "Parts_HighlightsItem";
                var settingsShapeName = part.GroupShapeName != "" ? "_" + part.GroupShapeName : "";
                return new ViewsInfos {
                    DefaultShapeName = defaultShapeName,
                    SuffixShapeName = settingsShapeName,
                    ResultShapeName = defaultShapeName + settingsShapeName
                };
            }
            return null;
        }

        public dynamic CreateHighlightsItemShape(dynamic item, string displayTemplate, string overrideShapeName="" ) {
            var dict = new Dictionary<string, object> { 
                    { "HighlightsItem", item },
                    { "DisplayTemplate", displayTemplate }
                };
            var args = Arguments.From(dict.Values, dict.Keys);
            var shapeName = ChooseView(item).ResultShapeName;
            if (!String.IsNullOrWhiteSpace(overrideShapeName)) {
                shapeName = overrideShapeName;
            }
            var shape = _shapeFactory.Create(shapeName, args);
            shape.Metadata.DisplayType = displayTemplate.ToString();
            return shape;

        }

        public dynamic MapContentToHighlightsItemViewModel(HighlightsItemPart itemPart) {
            var title = itemPart.ContentItem.As<TitlePart>();
            dynamic body = itemPart.Fields.First(f => f.Name == "MediaText");
            var mediaUrl = "";
            try {
                var mediaContainerPart = itemPart.ContentItem.Parts.Where(w => w.Fields.SingleOrDefault(w1 => w1.GetType() == typeof(MediaLibraryPickerField)) != null).FirstOrDefault();
                MediaLibraryPickerField mediafield = (MediaLibraryPickerField)mediaContainerPart.Fields.FirstOrDefault(w => w.GetType() == typeof(MediaLibraryPickerField));
                mediaUrl = mediafield.MediaParts.ToList()[0].MediaUrl;
            } catch { }
            string displayUrl = ContentItemExtensions.ItemDisplayUrl(new UrlHelper(new RequestContext(
             new HttpContextWrapper(HttpContext.Current),
             new RouteData()), RouteTable.Routes), itemPart);
            var HLItem = new HighlightsItemViewModel {
                Title = title != null ? title.Title : "",
                TitleSize = itemPart.TitleSize,
                SubTitle = itemPart.Sottotitolo,
                Body = body != null ? body.Value : "",
                ItemOrder = itemPart.ItemOrder,
                LinkTarget = itemPart.LinkTarget,
                LinkUrl = itemPart.LinkUrl,
                LinkText = itemPart.LinkText,
                GroupShapeName = itemPart.GroupShapeName,
                MediaUrl = mediaUrl,
                Video = itemPart.Video,
                GroupDisplayTemplate = itemPart.GroupDisplayTemplate,
                GroupDisplayPlugin = itemPart.GroupDisplayPlugin,
                Content = itemPart.ContentItem
            };
            return HLItem;
        }
        public IList<dynamic> MapContentToHighlightsItemsViewModel(IEnumerable<ContentItem> queryItems, Enums.DisplayTemplate displayTemplate, string displayPlugin, ViewsInfos viewsInfos) {
            var order = 0;
            IList<dynamic> fromQueryItems = new List<dynamic>();

            foreach (var queryItem in queryItems) {
                var title = queryItem.As<TitlePart>();
                var body = queryItem.As<BodyPart>();
                var bodyText = "";
                if (body == null) {
                    try {
                        var mediaContainerPart = queryItem.Parts.Where(w => w.Fields.SingleOrDefault(w1 => w1.GetType() == typeof(TextField)) != null).FirstOrDefault();
                        TextField textfield = (TextField)mediaContainerPart.Fields.FirstOrDefault(w => w.GetType() == typeof(TextField));
                        bodyText = textfield.Value;
                    } catch { }
                } else {
                    bodyText = body.Text;
                }
                var mediaUrl = "";
                try {
                    var mediaContainerPart = queryItem.Parts.Where(w => w.Fields.SingleOrDefault(w1 => w1.GetType() == typeof(MediaLibraryPickerField)) != null && w.GetType() == typeof(ContentPart)).FirstOrDefault();
                    MediaLibraryPickerField mediafield = (MediaLibraryPickerField)mediaContainerPart.Fields.FirstOrDefault(w => w.GetType() == typeof(MediaLibraryPickerField));
                    mediaUrl = mediafield.MediaParts.ToList()[0].MediaUrl;
                } catch { }
                string displayUrl = ContentItemExtensions.ItemDisplayUrl(new UrlHelper(new RequestContext(
                 new HttpContextWrapper(HttpContext.Current),
                 new RouteData()), RouteTable.Routes), queryItem);
                var HLItem = new HighlightsItemViewModel {
                    Title = title != null ? title.Title : "",
                    TitleSize = "",
                    Body = bodyText,
                    ItemOrder = order,
                    LinkTarget = Enums.LinkTargets._self,
                    LinkUrl = displayUrl,
                    GroupShapeName = viewsInfos.SuffixShapeName.Replace("_", ""),
                    LinkText = T("Details").Text,
                    MediaUrl = mediaUrl,
                    Video = false,
                    GroupDisplayTemplate = displayTemplate,
                    GroupDisplayPlugin = displayPlugin,
                    Content = queryItem
                };
                fromQueryItems.Add(HLItem);
                order++;
            }
            return fromQueryItems;
        }
        public IList<string> GetDisplayPluginsFor(Enums.DisplayTemplate displayTemplate) {
            ThemeEntry currentTheme = null;
            ExtensionDescriptor currentThemeDescriptor = _siteThemeService.GetSiteTheme();
            if (currentThemeDescriptor != null) {
                currentTheme = new ThemeEntry(currentThemeDescriptor);
            }
            System.Configuration.Configuration config;
            try {
                config = _localConfiguration.GetConfiguration(currentTheme.Descriptor.Path.StartsWith("~/Themes/") ? currentTheme.Descriptor.Path : "" + "~/Themes/" + currentTheme.Descriptor.Path);
                return config.AppSettings.Settings["DisplayPluginsFor" + displayTemplate.ToString()].Value.Split(new string[] { ",", ";", "|" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => displayTemplate + " - " + s.Trim()).ToList();
            } catch {
                return null;
            } finally {
                config = null;
            }

        }

        public string GetDisplayPluginsPreviewImage() {
            ThemeEntry currentTheme = null;
            ExtensionDescriptor currentThemeDescriptor = _siteThemeService.GetSiteTheme();
            if (currentThemeDescriptor != null) {
                currentTheme = new ThemeEntry(currentThemeDescriptor);
            }
            var previewPath = (currentTheme.Descriptor.Path.StartsWith("~/Themes/") ? currentTheme.Descriptor.Path : "" + "~/Themes/" + currentTheme.Descriptor.Path) + "/images/HighlightsDisplayPluginsPreview.jpg";
            currentTheme = null;
            if (File.Exists(_orchardServices.WorkContext.HttpContext.Server.MapPath(previewPath)))
                return previewPath;
            else
                return String.Empty;
        }
    }
}