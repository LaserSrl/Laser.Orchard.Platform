using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentTypes.Events;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.Models;
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
            // get all query
            model.QueryRecordEntries = GetQueriesRecordEntry();
            // managing the ids of selected queries
            if(!string.IsNullOrWhiteSpace(model.FilterQueryRecordId)) {
                var selectedIds = model.FilterQueryRecordId.Split(';');
                model.FilterQueryRecordsId = selectedIds;
            }
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "CacheEvictorPart") yield break;
            var model = new CacheEvictorPartSettings();
            model.QueryRecordEntries = GetQueriesRecordEntry();

            updateModel.TryUpdateModel(model, "CacheEvictorPartSettings", null, null);

            // get identity part of the ids for import/export
            var identityItems = string.Empty;
            if (!string.IsNullOrEmpty(model.EvictItem)) {
                var ids = model.EvictItem.Split(';');
                // get identitypart
                if (ids.Any()) {
                    int id;
                    foreach (var itemId in ids) {
                        if (!string.IsNullOrWhiteSpace(itemId)) {
                            if (int.TryParse(itemId, out id)) {
                                identityItems += GetIdentityPart(id) + ";";
                            } else {
                                Services.Notifier.Error(T("CacheEvictorPart - {0} is not an id", itemId));
                            }
                        }
                    }
                }
            }
            model.IdentityEvictItem = identityItems;


            // managing the ids of selected queries
            model.FilterQueryRecordId = string.Join(";", model.FilterQueryRecordsId);
            if (!string.IsNullOrWhiteSpace(model.FilterQueryRecordId)) {
                // get identitypart
                string identityQueryItems = string.Empty;

                if (model.FilterQueryRecordsId.Any()) {
                    foreach (var itemId in model.FilterQueryRecordsId) {
                        if (itemId == "-1") {
                            identityQueryItems += itemId+";";
                        } else {
                            identityQueryItems += GetIdentityPart(int.Parse(itemId)) + ";";
                        }
                    }
                }

                model.IdentityFilterQueryRecord = identityQueryItems;
            }

            // loads each settings field
            builder.WithSetting("CacheEvictorPartSettings.EvictItem", model.EvictItem);
            builder.WithSetting("CacheEvictorPartSettings.IdentityEvictItem", model.IdentityEvictItem);

            builder.WithSetting("CacheEvictorPartSettings.EvictTerms", model.EvictTerms.ToString());

            builder.WithSetting("CacheEvictorPartSettings.FilterQueryRecordId", model.FilterQueryRecordId);
            builder.WithSetting("CacheEvictorPartSettings.IdentityFilterQueryRecord", model.IdentityFilterQueryRecord);
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

        private IEnumerable<QueryRecordEntry> GetQueriesRecordEntry() {
            // populating the list of queries and layouts
            List<QueryRecordEntry> records = new List<QueryRecordEntry>();
            records.Add(new QueryRecordEntry {
                Id = -1,
                Name = T("No query").Text
            });

            records.AddRange(Services.ContentManager.Query<QueryPart, QueryPartRecord>().Join<TitlePartRecord>().OrderBy(x => x.Title).List()
                .Select(x => new QueryRecordEntry {
                    Id = x.Id,
                    Name = x.Name
                }));
            return records;
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

                var listEvictIds = GetIds(part.Settings.GetModel<CacheEvictorPartSettings>().IdentityEvictItem,true);
                var listEvictQueryIds = GetIds(part.Settings.GetModel<CacheEvictorPartSettings>().IdentityFilterQueryRecord,false);
                _contentDefinitionManager.AlterTypeDefinition(context.ContentTypeDefinition.Name, cfg => cfg
                      .WithPart(part.PartDefinition.Name,
                          pb => pb
                              .WithSetting("CacheEvictorPartSettings.EvictItem", listEvictIds)
                              .WithSetting("CacheEvictorPartSettings.FilterQueryRecordId", listEvictQueryIds)));
            }
        }

        public void ContentTypeImporting(ContentTypeImportingContext context) {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context) {
        }
        #endregion

        private string GetIds(string identityParts, bool isFreeText) {
            string listEvictIds = string.Empty;

            if (!string.IsNullOrEmpty(identityParts)) {
                foreach (var item in identityParts.Split(';')) {
                    // -1 identifies no selected query
                    if (!isFreeText && item == "-1") {
                        listEvictIds += "-1;";
                    }
                    var ciIdentity = _contentManager.ResolveIdentity(new ContentIdentity(item));
                    if (ciIdentity != null) {
                        listEvictIds += ciIdentity.Id.ToString() + ";";
                    }
                }
            }

            return listEvictIds;
        }
    }
}