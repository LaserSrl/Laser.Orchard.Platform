using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentTypes.Events;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Taxonomies.Services;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Settings {
    [OrchardFeature("Laser.Orchard.StartupConfig.CacheEvictorPart")]
    public class CacheEvictorPartSettingsHook : ContentDefinitionEditorEventsBase, IContentDefinitionEventHandler {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITaxonomyService _taxonomyService;

        public IOrchardServices Services { get; private set; }


        public CacheEvictorPartSettingsHook(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices services,
            ITaxonomyService taxonomyService) {

            Services = services;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _taxonomyService = taxonomyService;

            T = NullLocalizer.Instance;
        }

        public Localizer T;

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "CacheEvictorPart") yield break;
            var model = definition.Settings.GetModel<CacheEvictorPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "CacheEvictorPart") yield break;
            var model = new CacheEvictorPartSettings();
            updateModel.TryUpdateModel(model, "CacheEvictorPartSettings", null, null);

            // validate the inserted id
            if (!string.IsNullOrEmpty(model.EvictItem)) {
                int id;
                string identityItems = string.Empty;
                foreach (var item in model.EvictItem.Split(';')) {
                    if (!string.IsNullOrWhiteSpace(item)) {
                        if (int.TryParse(item, out id)) {
                            identityItems += GetIdentityPart(id) + ";";
                        }
                        else {
                            Services.Notifier.Error(T("CacheEvictorPart - {0} is not an id", item));
                        }
                    }
                }
                // if the validation was successful check the property the identity list
                model.IdentityEvictItem = identityItems;
            }

            // loads each settings field
            builder.WithSetting("CacheEvictorPartSettings.EvictItem", model.EvictItem);
            builder.WithSetting("CacheEvictorPartSettings.IdentityEvictItem", model.IdentityEvictItem);

            builder.WithSetting("CacheEvictorPartSettings.EvictTerms", model.EvictTerms.ToString());
        }

        private string GetIdentityPart(int id) {
            var content = _contentManager.Get(id);
            if (content != null) {
                var identity = _contentManager.GetItemMetadata(content).Identity;
                if (identity != null) {
                    return identity.ToString();
                }
                else {
                    Services.Notifier.Error(T("CacheEvictorPart - The loaded id {0} does not exist", id.ToString()));
                }
            }
            else {
                Services.Notifier.Error(T("CacheEvictorPart - The loaded id {0} does not exist", id.ToString()));
            }
            return string.Empty;
        }


        #region Implementation interface
        public void ContentFieldAttached(ContentFieldAttachedContext context) {
        }

        public void ContentFieldDetached(ContentFieldDetachedContext context) {
        }

        public void ContentPartAttached(ContentPartAttachedContext context) {
        }

        public void ContentPartCreated(ContentPartCreatedContext context) {
        }

        public void ContentPartDetached(ContentPartDetachedContext context) {
        }

        public void ContentPartImported(ContentPartImportedContext context) {
        }

        public void ContentPartImporting(ContentPartImportingContext context) {
        }

        public void ContentPartRemoved(ContentPartRemovedContext context) {
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context) {
        }

        public void ContentTypeImported(ContentTypeImportedContext context) {
            var part = context.ContentTypeDefinition.Parts
                .ToList()
                .Where(p => p.PartDefinition.Name == "CacheEvictorPart")
                .FirstOrDefault();
            if (part != null) {
                if (!string.IsNullOrEmpty(part.Settings.GetModel<CacheEvictorPartSettings>().IdentityEvictItem)) {
                    string listEvictIds = string.Empty;
                    foreach (var item in part.Settings.GetModel<CacheEvictorPartSettings>().IdentityEvictItem.Split(';')) {
                        var ciIdentity = _contentManager.ResolveIdentity(new ContentIdentity(item));
                        if (ciIdentity != null) {
                            listEvictIds += ciIdentity.Id.ToString() + ";";
                        }
                    }
                    _contentDefinitionManager.AlterTypeDefinition(context.ContentTypeDefinition.Name, cfg => cfg
                      .WithPart(part.PartDefinition.Name,
                          pb => pb
                              .WithSetting("CacheEvictorPartSettings.EvictItem", listEvictIds)));
                }
            }
        }

        public void ContentTypeImporting(ContentTypeImportingContext context) {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context) {
        }
        #endregion
    }
}