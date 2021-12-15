using System.Linq;
using System.Xml;
using Laser.Orchard.TemplateManagement.Extensions;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Laser.Orchard.TemplateManagement.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement.Drivers {
    [OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class TemplatePartDriver : ContentPartCloningDriver<TemplatePart> {
        private readonly IContentManager _contentManager;
        private readonly ITemplateService _templateService;

        public TemplatePartDriver(IContentManager contentManager, ITemplateService templateService) {
            _contentManager = contentManager;
            _templateService = templateService;
        }

        protected override string Prefix {
            get { return "Template"; }
        }

        protected override DriverResult Editor(TemplatePart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(TemplatePart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new TemplateViewModel {
                ExpectedParser = _templateService.SelectParser(part),
                Layouts = _templateService.GetLayouts().Where(x => x.Id != part.Id).ToList()
            };

            if (updater != null) {
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    part.Title = viewModel.Title.TrimSafe();
                    part.Subject = viewModel.Subject.TrimSafe();
                    part.Text = viewModel.Text;
                    part.Layout = viewModel.LayoutIdSelected != null ? _contentManager.Get<TemplatePart>(viewModel.LayoutIdSelected.Value) : null;
                    part.IsLayout = viewModel.IsLayout;
                    part.TemplateCode = viewModel.TemplateCode;
                }
            }
            else {
                viewModel.Title = part.Title;
                viewModel.Subject = part.Subject;
                viewModel.Text = part.Text;
                viewModel.LayoutIdSelected = part.Record.LayoutIdSelected;
                viewModel.IsLayout = part.IsLayout;
                viewModel.TemplateCode = part.TemplateCode;
            }

            return ContentShape("Parts_Template_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/Template", Model: viewModel, Prefix: Prefix));
        }

        protected override void Importing(TemplatePart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "Title", x => part.Title = x);
            context.ImportAttribute(part.PartDefinition.Name, "Subject", x => part.Subject = x);
            context.ImportAttribute(part.PartDefinition.Name, "Text", x => part.Text = x);
            context.ImportAttribute(part.PartDefinition.Name, "IsLayout", x => part.IsLayout = XmlConvert.ToBoolean(x));
            context.ImportAttribute(part.PartDefinition.Name, "Layout", x => {
                var layout = context.GetItemFromSession(x);

                if (layout != null && layout.Is<TemplatePart>()) {
                    part.Layout = layout.As<TemplatePart>();
                }
            });
            context.ImportAttribute(part.PartDefinition.Name, "TemplateCode", x => part.TemplateCode = x);
        }

        protected override void Exporting(TemplatePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Title", part.Title);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Subject", part.Subject);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Text", part.Text);
            context.Element(part.PartDefinition.Name).SetAttributeValue("IsLayout", part.IsLayout);

            if(part.Layout != null)
                context.Element(part.PartDefinition.Name).SetAttributeValue("Layout", context.ContentManager.GetItemMetadata(part.Layout).Identity.ToString());

            context.Element(part.PartDefinition.Name).SetAttributeValue("TemplateCode", part.TemplateCode);
        }

        protected override void Cloning(TemplatePart originalPart, TemplatePart clonePart, CloneContentContext context) {
            clonePart.Title = originalPart.Title;
            clonePart.Subject = originalPart.Subject;
            clonePart.Text = originalPart.Text;
            clonePart.IsLayout = originalPart.IsLayout;
            clonePart.Layout = originalPart.Layout;
            clonePart.TemplateCode = originalPart.TemplateCode;
        }
    }
}