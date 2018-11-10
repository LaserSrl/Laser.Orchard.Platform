using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement.Handlers {
    [OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class PartHandler : ContentHandler {
        private readonly ITemplateService _templateService;

        public PartHandler(IRepository<TemplatePartRecord> repository, ITemplateService templateService) {
            _templateService = templateService;
            Filters.Add(StorageFilter.For(repository));
            OnActivated<TemplatePart>(PropertyHandlers);
        }

        private void PropertyHandlers(ActivatedContentContext context, TemplatePart part) {
            part.LayoutField.Loader(() => part.Record.LayoutIdSelected != null ? _templateService.GetTemplate(part.Record.LayoutIdSelected.Value) : null);
            part.LayoutField.Setter(x => { part.Record.LayoutIdSelected = x != null ? x.Id : default(int?); return x; });
        }
    }
}