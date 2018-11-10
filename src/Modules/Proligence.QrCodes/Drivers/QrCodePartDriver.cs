namespace Proligence.QrCodes.Drivers {
    using Orchard;
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Drivers;
    using Orchard.ContentManagement.Handlers;
    using Orchard.Localization;
    using Orchard.UI.Notify;
    using Proligence.QrCodes.Models;
    using Proligence.QrCodes.Settings;

    public class QrCodePartDriver : ContentPartDriver<QrCodePart> {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/QrCode";

        public Localizer T { get; set; }
        private readonly IOrchardServices _orchardServices;
        public QrCodePartDriver(INotifier notifier, IOrchardServices orchardServices) {
            _notifier = notifier;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(QrCodePart part, string displayType, dynamic shapeHelper) {
            if (string.IsNullOrWhiteSpace(part.ActualValue)) return null;

            return ContentShape("Parts_QrCode",
                () => shapeHelper.Parts_QrCode(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(QrCodePart part, dynamic shapeHelper) {
            var settings = part.Settings.GetModel<QrCodeTypePartSettings>();
            if (part.Size == 0)
                part.Size = settings.Size;
            if (string.IsNullOrEmpty(part.Value))
                part.Value = settings.Value;
            return ContentShape("Parts_QrCode",
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(QrCodePart part, IUpdateModel updater, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(Permissions.EditQrCode)) {
                if (updater.TryUpdateModel(part, Prefix, null, null)) {
                    var settings = part.Settings.GetModel<QrCodeTypePartSettings>();

                    part.Record.Size = part.Size;
                    part.Record.Value = part.Value;

                    _notifier.Information(T("QR Code edited successfully"));
                }
                else {
                    _notifier.Error(T("Error during QR Code update!"));
                }
            }
            return Editor(part, shapeHelper);
        }

        protected override void Importing(QrCodePart part, ImportContentContext context) {
            var importedValue = context.Attribute(part.PartDefinition.Name, "Value");
            if (importedValue != null) {
                part.Value = importedValue;
            }

            var importedSize = context.Attribute(part.PartDefinition.Name, "Size");
            if (importedSize != null) {
                part.Size = int.Parse(importedSize);
            }

            var importedActualValue = context.Attribute(part.PartDefinition.Name, "ActualValue");
            if (importedActualValue != null) {
                part.ActualValue = importedActualValue;
            }
        }

        protected override void Exporting(QrCodePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Value", part.Value);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Size", part.Size);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ActualValue", part.ActualValue);
        }
    }
}