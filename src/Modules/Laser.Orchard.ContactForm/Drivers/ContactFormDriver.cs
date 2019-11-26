using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Laser.Orchard.ContactForm.Models;
using Laser.Orchard.ContactForm.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;
using Orchard.Data;
using Laser.Orchard.TemplateManagement.Models;
using Orchard.ContentManagement.MetaData.Models;
using System;

namespace Laser.Orchard.ContactForm.Drivers {
    public class ContactFormDriver : ContentPartCloningDriver<ContactFormPart> {

        private readonly IUtilsServices _utilsServices;
        private readonly IStorageProvider _storageProvider;
        private readonly INotifier _notifier;
        private readonly IRepository<TemplatePartRecord> _repositoryTemplatePartRecord;
        private readonly IContentManager _contentManager;
        private readonly IFrontEndEditService _frontEndEditService;

        public Localizer T { get; set; }

        public ContactFormDriver(
            IUtilsServices utilsServices, 
            INotifier notifier, 
            IStorageProvider storageProvider, 
            IRepository<TemplatePartRecord> repositoryTemplatePartRecord, 
            IContentManager contentManager,
            IFrontEndEditService frontEndProfileService) {

            _contentManager = contentManager;
            _storageProvider = storageProvider;
            _utilsServices = utilsServices;
            _notifier = notifier;
            _repositoryTemplatePartRecord = repositoryTemplatePartRecord;
            _frontEndEditService = frontEndProfileService;
        }

        Func<ContentTypePartDefinition, string, bool> OnlyShowReCaptcha = 
            (ctpd, typeName) => 
                ctpd.PartDefinition.Name == "ReCaptchaPart";
        Func<ContentPartFieldDefinition, bool> NoFields =
            (ctpd) =>
                false;

        /// <summary>
        /// Defines the shapes required for the part's main view.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="displayType">The display type.</param>
        /// <param name="shapeHelper">The shape helper.</param>
        protected override DriverResult Display(ContactFormPart part, string displayType, dynamic shapeHelper) {

            var viewModel = new ContactFormViewModel();
            if (part != null && displayType.Contains("Detail")) {
                viewModel.ContentRecordId = part.Record.Id;
                viewModel.ShowSubjectField = !part.UseStaticSubject;
                viewModel.ShowNameField = part.DisplayNameField;
                viewModel.RequireNameField = part.RequireNameField;
                viewModel.EnableFileUpload = part.EnableUpload;
                viewModel.AcceptPolicy = part.AcceptPolicy;
                viewModel.AcceptPolicyText = part.AcceptPolicyText;
                viewModel.AcceptPolicyUrl = part.AcceptPolicyUrl;
                viewModel.AcceptPolicyUrlText = part.AcceptPolicyUrlText;
                return ContentShape("Parts_ContactForm",
                    () => shapeHelper.Parts_ContactForm(
                        ContactForm: viewModel,
                        AdditionalShape: _frontEndEditService.BuildFrontEndShape(
                            _contentManager.BuildEditor(part),
                            OnlyShowReCaptcha,
                            NoFields)
                        ));

            }
            return null; // don't display if display type is not "detail"
        }

        /// <summary>
        /// Defines the shapes required for the editor view.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="shapeHelper">The shape helper.</param>
        protected override DriverResult Editor(ContactFormPart part, dynamic shapeHelper) {
            if (part == null)
                part = new ContactFormPart();

            var editModel = new ContactFormEditModel
            {
                BasePath = _utilsServices.VirtualMediaPath,
                ContactForm = part
            };

            return ContentShape("Parts_ContactForm_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ContactForm", Model: editModel, Prefix: Prefix));
        }

        /// <summary>
        /// Runs upon the POST of the editor view.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="updater">The updater.</param>
        /// <param name="shapeHelper">The shape helper.</param>
        protected override DriverResult Editor(ContactFormPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (part == null || updater == null)
                return Editor(null, shapeHelper);

            var editModel = new ContactFormEditModel {
                BasePath = _utilsServices.VirtualMediaPath,
                ContactForm = part
            };

            if (updater.TryUpdateModel(editModel, Prefix, null, null)) {
                if (!editModel.ContactForm.DisplayNameField) {
                    editModel.ContactForm.RequireNameField = false;
                }
                if (!string.IsNullOrWhiteSpace(editModel.ContactForm.PathUpload))
                {
                    if (!_storageProvider.FolderExists(editModel.ContactForm.PathUpload))
                    {
                        if (_storageProvider.TryCreateFolder(editModel.ContactForm.PathUpload))
                            _notifier.Information(T("The destination folder for the uploaded files has been succesfully created!"));
                        else
                            _notifier.Error(T("The destination folder for the uploaded files has not been created!"));
                    }
                }
            }

            return Editor(editModel.ContactForm, shapeHelper);
        }

        #region [ Import/Export ]
        protected override void Exporting(ContactFormPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("AttachFiles", part.AttachFiles);
            root.SetAttributeValue("DisplayNameField", part.DisplayNameField);
            root.SetAttributeValue("EnableUpload", part.EnableUpload);
            root.SetAttributeValue("PathUpload", part.PathUpload);
            root.SetAttributeValue("RecipientEmailAddress", part.RecipientEmailAddress);
            root.SetAttributeValue("RequireAttachment", part.RequireAttachment);
            root.SetAttributeValue("RequireNameField", part.RequireNameField);
            root.SetAttributeValue("StaticSubjectMessage", part.StaticSubjectMessage);
            root.SetAttributeValue("UseStaticSubject", part.UseStaticSubject);
            root.SetAttributeValue("ThankyouPage", part.ThankyouPage);
            root.SetAttributeValue("AcceptPolicy", part.AcceptPolicy);
            root.SetAttributeValue("AcceptPolicyUrl", part.AcceptPolicyUrl);
            root.SetAttributeValue("AcceptPolicyUrlText", part.AcceptPolicyUrlText);
            root.SetAttributeValue("AcceptPolicyText", part.AcceptPolicyText);
            if (part.TemplateRecord_Id > 0) 
            {
                //cerco il corrispondente valore dell' identity dalla parts del template e lo associo al campo Layout 
                var contItemTempl=_contentManager.Get(part.TemplateRecord_Id);
                if (contItemTempl != null) {
                    root.SetAttributeValue("TemplateRecord_Id", _contentManager.GetItemMetadata(contItemTempl).Identity.ToString());
                }
            }
        }

        protected override void Importing(ContactFormPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var AttachFiles = root.Attribute("AttachFiles");
            if (AttachFiles != null) {
                part.AttachFiles = bool.Parse(AttachFiles.Value);
            }
            var DisplayNameField = root.Attribute("DisplayNameField");
            if (DisplayNameField != null) {
                part.DisplayNameField = bool.Parse(DisplayNameField.Value);
            }
            var EnableUpload = root.Attribute("EnableUpload");
            if (EnableUpload != null) {
                part.EnableUpload = bool.Parse(EnableUpload.Value);
            }
            var PathUpload = root.Attribute("PathUpload");
            if (PathUpload != null) {
                part.PathUpload = PathUpload.Value;
            }
            var RecipientEmailAddress = root.Attribute("RecipientEmailAddress");
            if (RecipientEmailAddress != null) {
                part.RecipientEmailAddress = RecipientEmailAddress.Value;
            }
            var RequireAttachment = root.Attribute("RequireAttachment");
            if (RequireAttachment != null) {
                part.RequireAttachment = bool.Parse(RequireAttachment.Value);
            }
            var RequireNameField = root.Attribute("RequireNameField");
            if (RequireNameField != null) {
                part.RequireNameField = bool.Parse(RequireNameField.Value);
            }
            var StaticSubjectMessage = root.Attribute("StaticSubjectMessage");
            if (StaticSubjectMessage != null) {
                part.StaticSubjectMessage = StaticSubjectMessage.Value;
            }
            var UseStaticSubject = root.Attribute("UseStaticSubject");
            if (UseStaticSubject != null) {
                part.UseStaticSubject = bool.Parse(UseStaticSubject.Value);
            }
            var ThankyouPage = root.Attribute("ThankyouPage");
            if (ThankyouPage != null) {
                part.ThankyouPage = ThankyouPage.Value;
            }
            var AcceptPolicy = root.Attribute("AcceptPolicy");
            if (UseStaticSubject != null) {
                part.AcceptPolicy = bool.Parse(AcceptPolicy.Value);
            }
            var AcceptPolicyUrl = root.Attribute("AcceptPolicyUrl");
            if (AcceptPolicyUrl != null) {
                part.AcceptPolicyUrl = AcceptPolicyUrl.Value;
            }
            var AcceptPolicyUrlText = root.Attribute("AcceptPolicyUrlText");
            if (AcceptPolicyUrlText != null) {
                part.AcceptPolicyUrlText = AcceptPolicyUrlText.Value;
            }
            var AcceptPolicyText = root.Attribute("AcceptPolicyText");
            if (AcceptPolicyText != null) {
                part.AcceptPolicyText = AcceptPolicyText.Value;
            }

            context.ImportAttribute(part.PartDefinition.Name, "TemplateRecord_Id", (x) => {
                var template = context.GetItemFromSession(x);
                if (template != null && template.Has<TemplatePart>()) {
                    part.TemplateRecord_Id = template.Id;
                }
            });
        }
        #endregion

        protected override void Cloning(ContactFormPart originalPart, ContactFormPart clonePart, CloneContentContext context) {
            clonePart.RecipientEmailAddress = originalPart.RecipientEmailAddress;
            clonePart.StaticSubjectMessage = originalPart.StaticSubjectMessage;
            clonePart.UseStaticSubject = originalPart.UseStaticSubject;
            clonePart.DisplayNameField = originalPart.DisplayNameField;
            clonePart.RequireNameField = originalPart.RequireNameField;
            clonePart.TemplateRecord_Id = originalPart.TemplateRecord_Id;
            clonePart.EnableUpload = originalPart.EnableUpload;
            clonePart.AttachFiles = originalPart.AttachFiles;
            clonePart.PathUpload = originalPart.PathUpload;
            clonePart.RequireAttachment = originalPart.RequireAttachment;
            clonePart.ThankyouPage = originalPart.ThankyouPage;
            clonePart.AcceptPolicy = originalPart.AcceptPolicy;
            clonePart.AcceptPolicyUrl = originalPart.AcceptPolicyUrl;
            clonePart.AcceptPolicyUrlText = originalPart.AcceptPolicyUrlText;
            clonePart.AcceptPolicyText = originalPart.AcceptPolicyText;
        }
    }
}