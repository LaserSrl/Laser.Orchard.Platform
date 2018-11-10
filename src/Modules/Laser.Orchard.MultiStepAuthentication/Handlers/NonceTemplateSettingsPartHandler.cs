using Laser.Orchard.MultiStepAuthentication.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.MultiStepAuthentication.Handlers {
    [OrchardFeature("Laser.Orchard.NonceTemplateEmail")]
    public class NonceTemplateSettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }

        private readonly ITemplateService _templateService;
        public NonceTemplateSettingsPartHandler(IRepository<NonceTemplateSettingsPartRecord> repository, ITemplateService templateService) {
            T = NullLocalizer.Instance;
            _templateService = templateService;

            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<NonceTemplateSettingsPart>("Site"));
            //  Filters.Add(new TemplateFilterForPart<NonceTemplateSettingsPart>("NonceTemplateSettings", "Parts/NonceTemplateSettings", "NonceLoginSettings"));
        //    OnGetContentItemMetadata<NonceTemplateSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("NonceLoginSettings"))));
            OnActivated<NonceTemplateSettingsPart>(PropertyHandlers);
        }

        private void PropertyHandlers(ActivatedContentContext context, NonceTemplateSettingsPart part) {
            part.SelectedTemplateField.Loader(() => part.Record.TemplateIdSelected != null ? _templateService.GetTemplate(part.Record.TemplateIdSelected.Value) : null);
            part.SelectedTemplateField.Setter(x => { part.Record.TemplateIdSelected = x != null ? x.Id : default(int?); return x; });
        }
    }
}