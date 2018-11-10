using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement.Handlers {
    [OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class CustomTemplatePickerPartHandler : ContentHandler {
        private readonly ITemplateService _templateService;

        public CustomTemplatePickerPartHandler(IRepository<CustomTemplatePickerPartRecord> repository, ITemplateService templateService) {
            _templateService = templateService;
            Filters.Add(StorageFilter.For(repository));
            OnActivated<CustomTemplatePickerPart>(PropertyHandlers);
        }

        private void PropertyHandlers(ActivatedContentContext context, CustomTemplatePickerPart part) {
            part.SelectedTemplateField.Loader(() => part.Record.TemplateIdSelected != null ? _templateService.GetTemplate(part.Record.TemplateIdSelected.Value) : null);
            part.SelectedTemplateField.Setter(x => { part.Record.TemplateIdSelected = x != null ? x.Id : default(int?); return x; });
        }
    }
}