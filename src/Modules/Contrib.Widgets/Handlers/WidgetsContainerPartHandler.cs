using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Contrib.Widgets.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Records;
using Orchard.Localization.Services;
using Orchard.Localization.ViewModels;
using Orchard.Logging;
using Orchard.Mvc.Routes;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;

namespace Contrib.Widgets.Handlers {
    public class WidgetsContainerPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IWidgetManager _widgetManager;
        private readonly ILocalizationService _localizationService;
        private readonly ShellSettings _shellSettings;
        private readonly ITaxonomyService _taxonomyService;
        private readonly UrlPrefix _urlPrefix;
        public Localizer T { get; set; }

        public WidgetsContainerPartHandler(
            IContentManager contentManager,
            IWidgetManager widgetManager,
            ILocalizationService localizationService,
            ShellSettings shellSettings,
            ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _widgetManager = widgetManager;
            _localizationService = localizationService;
            _shellSettings = shellSettings;
            _taxonomyService = taxonomyService;
            if (!string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                _urlPrefix = new UrlPrefix(_shellSettings.RequestUrlPrefix);
            T = NullLocalizer.Instance;

            OnRemoved<WidgetsContainerPart>((context, part) => {
                DeleteWidgets(part);
            });
            OnUpdateEditorShape<WidgetsContainerPart>((context, part) => {
                var lPart = part.ContentItem.As<LocalizationPart>();
                if(lPart != null) {
                    var settings = part.Settings.GetModel<WidgetsContainerSettings>();
                    if (settings.TryToLocalizeItems) {
                        var culture = lPart.Culture;
                        var widgets = _widgetManager.GetWidgets(part.ContentItem.Id, part.ContentItem.IsPublished())
                            .Where(p => p.ContentItem.Has<LocalizationPart>() 
                            && p.ContentItem.Get<LocalizationPart>().Culture == null);
                        foreach (var widget in widgets) {
                            var ci = widget.ContentItem;
                            _localizationService.SetContentCulture(ci, culture.Culture);
                            // manage taxonomy field out of the normal flow:
                            // gets translations of selected terms in taxonomy fields before BuildEditor()
                            var translatedTaxo = TranslateTaxonomies(ci, culture, _localizationService);

                            // simulates a user that opens in edit model the widget and saves it 
                            // to trigger all handlers and drivers
                            var shapeWidget = _contentManager.BuildEditor(ci);
                            var shapeUpdate = _contentManager.UpdateEditor(ci, new CustomUpdater(shapeWidget, culture.Culture, Logger));

                            // sets translated terms in taxonomy fields after UpdateEditor()
                            ApplyTranslatedTaxonomies(ci, translatedTaxo, _taxonomyService);

                            ci.VersionRecord.Published = false;
                            _contentManager.Publish(ci);
                        }
                    }
                }
            });
        }
        // static method because it is called in the contructor
        private static Dictionary<string, List<TermPart>> TranslateTaxonomies(ContentItem ci, CultureRecord culture, ILocalizationService localizationService) {
            var translations = new Dictionary<string, List<TermPart>>();
            var taxoFields = ci.Parts.SelectMany(p => p.Fields.Where(f => f is TaxonomyField).Select(f => f as TaxonomyField));
            foreach(var field in taxoFields) {
                var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldLocalizationSettings>();
                if(settings != null && settings.TryToLocalize) {
                    // translate terms
                    var newTerms = new List<TermPart>();
                    foreach (var term in field.Terms) {
                        // adds translated term if it exists (same ogic of LocalizedTaxonomyFieldHandler.BuildEditorShape)
                        var translatedTerm = localizationService.GetLocalizedContentItem(term, culture.Culture);
                        if(translatedTerm != null) {
                            newTerms.Add(translatedTerm.ContentItem.As<TermPart>());
                        }
                    }
                    translations.Add(field.PartFieldDefinition.Name, newTerms);
                }
                else {
                    // copy terms
                    var newTerms = new List<TermPart>();
                    foreach (var term in field.Terms) {
                        newTerms.Add(term.ContentItem.As<TermPart>());
                    }
                    translations.Add(field.PartFieldDefinition.Name, newTerms);
                }
            }
            return translations;
        }
        // static method because it is called in the contructor
        private static void ApplyTranslatedTaxonomies(ContentItem ci, Dictionary<string, List<TermPart>> translations, ITaxonomyService taxonomyService) {
            var taxoFields = ci.Parts.SelectMany(p => p.Fields.Where(f => f is TaxonomyField).Select(f => f as TaxonomyField));
            foreach (var field in taxoFields) {
                if (translations.ContainsKey(field.PartFieldDefinition.Name)) {
                    taxonomyService.UpdateTerms(ci, translations[field.PartFieldDefinition.Name], field.PartFieldDefinition.Name);
                }
            }
        }
        private void DeleteWidgets(WidgetsContainerPart part) {
            var contentItem = part.ContentItem;

            var widgets = _widgetManager.GetWidgets(contentItem.Id, false);
            foreach (var w in widgets) {
                _contentManager.Remove(w.ContentItem);
            }
        }
        private class CustomUpdater : IUpdateModel {
            private readonly Dictionary<string, dynamic> _shapes;
            private readonly string _culture;
            private readonly ILogger _logger;
            public CustomUpdater(dynamic shape, string culture, ILogger loggger) {
                _shapes = new Dictionary<string, dynamic>();
                _culture = culture;
                _logger = loggger;
                foreach(var item in shape.Content.Items) {
                    try {
                        var key = ComposeKey(item);
                        if( !_shapes.ContainsKey(key)) {
                            if(key.StartsWith("- ")) {
                                _shapes.Add(key, item);
                            }
                            else {
                                _shapes.Add(key, item.Model);
                            }
                        }
                    }
                    catch {
                        // ignore problems due to dynamic object without the expected properties
                    }
                }
            }
            public void AddModelError(string key, LocalizedString errorMessage) {
            }

            public bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class {
                try {
                    var key = ComposeKey(model, prefix);
                    // does not affect LocalizationPart
                    if (_shapes.ContainsKey(key) && (model != null) && !(model is EditLocalizationViewModel)) {
                        // copies each property without modifing 'model' variable
                        try {
                            foreach (var prop in model.GetType().GetProperties()) {
                                if(prop.CanRead && prop.CanWrite) {
                                    prop.SetValue(model, prop.GetValue(_shapes[key]));
                                }
                            }
                        }
                        catch(Exception e) {
                            _logger.Error(e, "Error copying properties into model.");
                        }
                    }
                }
                catch(Exception e) {
                    _logger.Error(e, "Error in TryUpdateModel() of EmptyUpdater.");
                }
                return true;
            }
            private string ComposeKey(dynamic item) {
                try {
                    return ComposeKey(item.Model, item.Prefix);
                }
                catch {
                    return ComposeKey(item, null);
                }
            }
            private string ComposeKey(dynamic model, string prefix) {
                var result = "";
                try {
                    result = string.Format("{0} {1}", prefix ?? "-", model.GetType().Name);
                }
                catch {
                    result = string.Format("- {1}", model.GetType().Name);
                }
                return result;
            }
        }
    }
}