using AutoMapper;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Mobile.Settings;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.Drivers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class MobilePushPartDriver : ContentPartCloningDriver<MobilePushPart> {

        private readonly IOrchardServices _orchardServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IRepository<PushNotificationRecord> _repoPushNotification;
        private readonly ShellSettings _shellSettings;
        private readonly IPushGatewayService _pushGatewayService;
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Mobile.MobilePush"; }
        }

        public MobilePushPartDriver(IOrchardServices orchardServices, IControllerContextAccessor controllerContextAccessor,
                                    IRepository<PushNotificationRecord> repoPushNotification, ShellSettings shellSettings,
                                    IPushGatewayService pushGatewayService) {
            _orchardServices = orchardServices;
            _controllerContextAccessor = controllerContextAccessor;
            _repoPushNotification = repoPushNotification;
            _shellSettings = shellSettings;
            _pushGatewayService = pushGatewayService;
        }

        protected override DriverResult Display(MobilePushPart part, string displayType, dynamic shapeHelper)
        {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin)
            {
                if (displayType == "Summary")
                {
                    return ContentShape("Parts_MobilePush",
                        () => shapeHelper.Parts_MobilePush(ToPush: part.ToPush, PushSent: part.PushSent, TargetDeviceNumber: part.TargetDeviceNumber, PushSentNumber: part.PushSentNumber));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        protected override DriverResult Editor(MobilePushPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);

        }

        protected override DriverResult Editor(MobilePushPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new MobilePushVM();
            viewModel.PartId = part.Id;
            var pushSettings = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>();
            viewModel.ShowTestOptions = pushSettings.ShowTestOptions;
            if (updater != null) {
                if (viewModel.ShowTestOptions == false)
                    viewModel.TestPush = false;
                // We are in "postback" mode, so update our part
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    // forza il valore di ToPush che altrimenti sembra non venire aggiornato correttamente
                    if (viewModel.PushSent)
                    {
                        viewModel.ToPush = true;
                    }
                    Mapper.Initialize(cfg => {
                        cfg.CreateMap<MobilePushVM, MobilePushPart>();
                    });
                    Mapper.Map<MobilePushVM, MobilePushPart>(viewModel, part);

                } else
                    updater.AddModelError("Cannotupdate", T("Cannot Update!"));
            } else {
                //   viewModel.ListCountries = _countriesService.GetAllNazione();
                // We are in render mode (not postback), so initialize our view model.
                //Mapper.CreateMap<MobilePushPart, MobilePushVM>();

                //Mapper.Map(part, viewModel);
                viewModel.TitlePush = part.TitlePush;
                viewModel.TextPush = part.TextPush;
                viewModel.ToPush = part.ToPush;
                viewModel.TestPush = part.TestPush;
                viewModel.DevicePush = part.DevicePush;
                viewModel.TestPushToDevice = part.TestPushToDevice;
                viewModel.DevicePush = part.DevicePush;
                viewModel.PushSent = part.PushSent;
                viewModel.TargetDeviceNumber = part.TargetDeviceNumber;
                viewModel.PushSentNumber = part.PushSentNumber;
                viewModel.UseRecipientList = part.UseRecipientList;
                viewModel.RecipientList = part.RecipientList;
            }
            viewModel.SiteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl + "/" + _shellSettings.RequestUrlPrefix;
            viewModel.HideRelated = part.Settings.GetModel<PushMobilePartSettingVM>().HideRelated;
            _controllerContextAccessor.Context.Controller.TempData["HideRelated"] = viewModel.HideRelated;
            // Valorizzo TextNumberPushTest
            viewModel.PushTestNumber = _repoPushNotification.Count(x => x.Produzione == false);
            viewModel.ListOfDevice = GetListOfDeviceTypes(pushSettings);
            viewModel.SentCounters = _pushGatewayService.GetNotificationsCounters(part.ContentItem);
            if (part.ContentItem.ContentType == "CommunicationAdvertising") {
                // Flag Approvato all'interno del tab
                viewModel.PushAdvertising = true;
                return ContentShape("Parts_MobilePush_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobilePush_Edit", Model: viewModel, Prefix: Prefix));
            } 
            else {
                // Flag Approvato in fondo
                viewModel.PushAdvertising = false;

                var shapes = new List<DriverResult>();
                shapes.Add(ContentShape("Parts_MobilePush_Edit",
                                 () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobilePush_Edit",
                                     Model: viewModel,
                                     Prefix: Prefix)));
                shapes.Add(ContentShape("Parts_MobilePushApproved_Edit",
                                 () => shapeHelper.EditorTemplate(TemplateName: "Parts/MobilePushApproved_Edit",
                                     Model: viewModel,
                                     Prefix: Prefix)));

                return new CombinedResult(shapes);
            }
        }
        private SelectList GetListOfDeviceTypes(PushMobileSettingsPart pushSettings) {
            var _list = new List<SelectListItem>();
            if(string.IsNullOrWhiteSpace(pushSettings.AndroidApiKey) == false) {
                _list.Add(new SelectListItem() { Value = TipoDispositivo.Android.ToString(), Text = TipoDispositivo.Android.ToString() });
            }
            if (string.IsNullOrWhiteSpace(pushSettings.ApplePathCertificateFile) == false) {
                _list.Add(new SelectListItem() { Value = TipoDispositivo.Apple.ToString(), Text = TipoDispositivo.Apple.ToString() });
            }
            if (string.IsNullOrWhiteSpace(pushSettings.WindowsAppPackageName) == false) {
                _list.Add(new SelectListItem() { Value = TipoDispositivo.WindowsMobile.ToString(), Text = TipoDispositivo.WindowsMobile.ToString() });
            }
            _list.Insert(0, new SelectListItem() { Value = "All", Text = "All" });
            return new SelectList((IEnumerable<SelectListItem>)_list, "Value", "Text");
        }
        protected override void Importing(MobilePushPart part, ImportContentContext context) {
            var TitlePush = context.Attribute(part.PartDefinition.Name, "TitlePush");
            if (TitlePush != null)
                part.TitlePush = TitlePush;
            var TextPush = context.Attribute(part.PartDefinition.Name, "TextPush");
            if (TextPush != null)
                part.TextPush = TextPush;
            //ToPush non ha senso per l'import
            var TestPush = context.Attribute(part.PartDefinition.Name, "TestPush");
            if (TestPush != null)
                part.TestPush = bool.Parse(TestPush);
            var TestPushToDevice = context.Attribute(part.PartDefinition.Name, "TestPushToDevice");
            if (TestPushToDevice != null)
                part.TestPushToDevice = bool.Parse(TestPushToDevice);
            var DevicePush = context.Attribute(part.PartDefinition.Name, "DevicePush");
            if (DevicePush != null)
                part.DevicePush = DevicePush;
            var UseRecipientList = context.Attribute(part.PartDefinition.Name, "UseRecipientList");
            if (UseRecipientList != null)
                part.UseRecipientList = bool.Parse(UseRecipientList);
            var RecipientList = context.Attribute(part.PartDefinition.Name, "RecipientList");
            if (RecipientList != null)
                part.RecipientList = RecipientList;
            //PushSent non ha senso per l'import
            //TargetDeviceNumber non ha senso per l'import
            //PushSentNumber non ha senso per l'import
        }

        protected override void Exporting(MobilePushPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("TitlePush", part.TitlePush);
            root.SetAttributeValue("TextPush", part.TextPush);
            //ToPush non ha senso per l'import
            root.SetAttributeValue("TestPush", part.TestPush);
            root.SetAttributeValue("TestPushToDevice", part.TestPushToDevice);
            root.SetAttributeValue("DevicePush", part.DevicePush);
            root.SetAttributeValue("UseRecipientList", part.UseRecipientList);
            root.SetAttributeValue("RecipientList", part.RecipientList);
            //PushSent non ha senso per l'import
            //TargetDeviceNumber non ha senso per l'import
            //PushSentNumber non ha senso per l'import
        }

        protected override void Cloning(MobilePushPart originalPart, MobilePushPart clonePart, CloneContentContext context) {
            clonePart.TitlePush = originalPart.TitlePush;
            clonePart.TextPush = originalPart.TextPush;
            //The ToPush property is not copied over and left to default (false), else the handler will send the notifications
            clonePart.TestPush = originalPart.TestPush;
            clonePart.TestPushToDevice = originalPart.TestPushToDevice;
            clonePart.DevicePush = originalPart.DevicePush;
            clonePart.UseRecipientList = originalPart.UseRecipientList;
            clonePart.RecipientList = originalPart.RecipientList;
            //PushSent is not copied over, because it is controlled by the push services
            //TargetDeviceNumber is not copied over, because it is controlled by the push services
            //PushSentNumber is not copied over, because it is controlled by the push services
        }
    }
}