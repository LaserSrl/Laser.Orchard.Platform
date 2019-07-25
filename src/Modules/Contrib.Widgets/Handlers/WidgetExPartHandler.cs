using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.PublishLater.Models;

namespace Contrib.Widgets.Handlers {
    [OrchardFeature("Contrib.Widgets")]
    public class WidgetExPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public WidgetExPartHandler(IRepository<WidgetExPartRecord> repository, IContentDefinitionManager contentDefinitionManager, IContentManager contentManager) {
            Filters.Add(StorageFilter.For(repository));
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            OnActivated<WidgetExPart>(SetupFields);
            OnUpdated<WidgetExPart>(PublishWidget);
        }

        private void SetupFields(ActivatedContentContext context, WidgetExPart part) {
            part.HostField.Loader(() => part.Record.HostId != null ? _contentManager.Get(part.Record.HostId.Value) : null);
            part.HostField.Setter(x => {
                part.Record.HostId = x != null ? x.Id : default(int?);
                return x;
            });
        }

        private void PublishWidget(UpdateContentContext context, WidgetExPart part) {
            if (!context.ContentItem.TypeDefinition.Settings.ContainsKey("Stereotype") || context.ContentItem.TypeDefinition.Settings["Stereotype"] != "Widget" || part.ContentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable || part.ContentItem.Has<PublishLaterPart>())
                return;

            _contentManager.Publish(part.ContentItem);
        }

        protected override void Activated(ActivatedContentContext context) {
            if (!context.ContentItem.TypeDefinition.Settings.ContainsKey("Stereotype") || context.ContentItem.TypeDefinition.Settings["Stereotype"] != "Widget")
                return;

            if (!context.ContentItem.Is<WidgetExPart>()) {
                _contentDefinitionManager.AlterTypeDefinition(context.ContentType, type => type.WithPart("WidgetExPart"));
            }
        }
    }
}