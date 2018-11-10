using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.TemplateManagement.Drivers {
    public class CustomTemplatePickerPartDriver : ContentPartCloningDriver<CustomTemplatePickerPart> {
        private readonly IContentManager _contentManager;
        private readonly ITemplateService _templateService;

        public CustomTemplatePickerPartDriver(IContentManager contentManager, ITemplateService templateService) {
            _contentManager = contentManager;
            _templateService = templateService;
        }
        protected override string Prefix {
            get { return "CustomTemplatePicker"; }
        }

        protected override DriverResult Editor(CustomTemplatePickerPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CustomTemplatePickerPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vModel = new CustomTemplatePickerViewModel {
                TemplateIdSelected = part.SelectedTemplate != null ? part.SelectedTemplate.Id : (int?)null,
                TemplatesList = _templateService.GetTemplates()
            };
            if (updater != null) {
                if (updater.TryUpdateModel(vModel, Prefix, null, null)) {
                    part.SelectedTemplate = _contentManager.Get<TemplatePart>(vModel.TemplateIdSelected.Value);
                } 
            }
            return ContentShape("Parts_CustomTemplatePicker_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/CustomTemplatePicker_Edit", Model: vModel, Prefix: Prefix));
        }

        protected override void Importing(CustomTemplatePickerPart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "SelectedTemplate", x => {
                var itemFromid = context.GetItemFromSession(x);
                if (itemFromid != null && itemFromid.Is<TemplatePart>()) {
                    part.SelectedTemplate = itemFromid.As<TemplatePart>();
                }
            });
        }
        protected override void Exporting(CustomTemplatePickerPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            if (part.SelectedTemplate != null) {
                var templateIdentity = _contentManager.GetItemMetadata(part.SelectedTemplate.ContentItem).Identity.ToString();
                root.SetAttributeValue("SelectedTemplate", templateIdentity);
            }         
        }

        protected override void Cloning(CustomTemplatePickerPart originalPart, CustomTemplatePickerPart clonePart, CloneContentContext context) {
            clonePart.SelectedTemplate = originalPart.SelectedTemplate;
        }
    }
}