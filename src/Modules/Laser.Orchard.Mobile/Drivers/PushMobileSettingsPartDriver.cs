using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.Hosting;

namespace Laser.Orchard.Mobile.Drivers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushMobileSettingsPartDriver : ContentPartDriver<PushMobileSettingsPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly ITaxonomyService _taxonomyService;
        public PushMobileSettingsPartDriver(IOrchardServices orchardServices, ShellSettings shellSettings, ITaxonomyService taxonomyService) {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
            _taxonomyService = taxonomyService;
            string mobile_folder = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSettings.Name + @"\Mobile\";
            if (!System.IO.Directory.Exists(mobile_folder))
                System.IO.Directory.CreateDirectory(mobile_folder);
        }
        protected override string Prefix {
            get { return "Laser.Mobile.Settings"; }
        }


        protected override DriverResult Editor(PushMobileSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);

        }



        protected override DriverResult Editor(PushMobileSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            return ContentShape("Parts_PushMobileSettings_Edit", () => {
                var viewModel = new PushMobileSettingsVM();
                viewModel.AppleCertificateTenant = _shellSettings.Name;
                var getpart = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>();
                viewModel.AndroidApiKey = getpart.AndroidApiKey;
                viewModel.AndroidApiKeyDevelopment = getpart.AndroidApiKeyDevelopment;
                viewModel.AndroidPushServiceUrl = getpart.AndroidPushServiceUrl;
                viewModel.AndroidPushNotificationIcon = getpart.AndroidPushNotificationIcon;
                viewModel.AppleCertificatePassword = getpart.AppleCertificatePassword;
                viewModel.ApplePathCertificateFile = getpart.ApplePathCertificateFile;
                viewModel.AppleCertificatePasswordDevelopment = getpart.AppleCertificatePasswordDevelopment;
                viewModel.ApplePathCertificateFileDevelopment = getpart.ApplePathCertificateFileDevelopment;
                viewModel.WindowsAppPackageName = getpart.WindowsAppPackageName;
                viewModel.WindowsAppSecurityIdentifier = getpart.WindowsAppSecurityIdentifier;
                viewModel.WindowsEndPoint = getpart.WindowsEndPoint;
                viewModel.ShowTestOptions = getpart.ShowTestOptions;
                viewModel.ApplePushSound = getpart.ApplePushSound;
                viewModel.TaxonomyName = getpart.TaxonomyName;
                viewModel.PushSendBufferSize = getpart.PushSendBufferSize;
                viewModel.CommitSentOnly = getpart.CommitSentOnly;
                viewModel.DelayMinutesBeforeRetry = getpart.DelayMinutesBeforeRetry;
                viewModel.MaxNumRetry = getpart.MaxNumRetry;
                viewModel.MaxPushPerIteration = getpart.MaxPushPerIteration;

                List<TaxonomyPart> tps = _taxonomyService.GetTaxonomies().ToList();
                IEnumerable<ListItem> selectList =
                from c in tps
                select new ListItem {
                    Selected = (c.Id.ToString() == viewModel.TaxonomyName),
                    Text = c.Name,
                    Value = c.Id.ToString()
                };
                viewModel.ListOfTaxonomies = new SelectList(selectList.ToList(), "Value", "Text", viewModel.TaxonomyName);

                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        part.AndroidApiKey = viewModel.AndroidApiKey;
                        part.AndroidApiKeyDevelopment = viewModel.AndroidApiKeyDevelopment;
                        part.AndroidPushServiceUrl = viewModel.AndroidPushServiceUrl;
                        part.AndroidPushNotificationIcon = viewModel.AndroidPushNotificationIcon;
                        part.AppleCertificatePassword = viewModel.AppleCertificatePassword;
                        part.ApplePathCertificateFile = viewModel.ApplePathCertificateFile;
                        part.ApplePushSound = viewModel.ApplePushSound;
                        part.AppleCertificatePasswordDevelopment = viewModel.AppleCertificatePasswordDevelopment;
                        part.ApplePathCertificateFileDevelopment = viewModel.ApplePathCertificateFileDevelopment;
                        part.WindowsAppPackageName = viewModel.WindowsAppPackageName;
                        part.WindowsAppSecurityIdentifier = viewModel.WindowsAppSecurityIdentifier;
                        part.WindowsEndPoint = viewModel.WindowsEndPoint;
                        part.ShowTestOptions = viewModel.ShowTestOptions;
                        part.TaxonomyName = viewModel.TaxonomyName;
                        part.PushSendBufferSize = viewModel.PushSendBufferSize;
                        part.CommitSentOnly = viewModel.CommitSentOnly;
                        part.DelayMinutesBeforeRetry = viewModel.DelayMinutesBeforeRetry;
                        part.MaxNumRetry = viewModel.MaxNumRetry;
                        part.MaxPushPerIteration = viewModel.MaxPushPerIteration;
                    }
                }
                else {
                    viewModel.AndroidApiKey = part.AndroidApiKey;
                    viewModel.AndroidApiKeyDevelopment = part.AndroidApiKeyDevelopment;
                    viewModel.AndroidPushServiceUrl = part.AndroidPushServiceUrl;
                    viewModel.AndroidPushNotificationIcon = part.AndroidPushNotificationIcon;
                    viewModel.AppleCertificatePassword = part.AppleCertificatePassword;
                    viewModel.ApplePathCertificateFile = part.ApplePathCertificateFile;
                    viewModel.AppleCertificatePasswordDevelopment = part.AppleCertificatePasswordDevelopment;
                    viewModel.ApplePathCertificateFileDevelopment = part.ApplePathCertificateFileDevelopment;
                    viewModel.WindowsAppPackageName = part.WindowsAppPackageName;
                    viewModel.WindowsAppSecurityIdentifier = part.WindowsAppSecurityIdentifier;
                    viewModel.WindowsEndPoint = part.WindowsEndPoint;
                    viewModel.ShowTestOptions = part.ShowTestOptions;
                    viewModel.ApplePushSound = part.ApplePushSound;
                    viewModel.TaxonomyName = part.TaxonomyName;
                    viewModel.PushSendBufferSize = part.PushSendBufferSize;
                    viewModel.CommitSentOnly = part.CommitSentOnly;
                    viewModel.DelayMinutesBeforeRetry = part.DelayMinutesBeforeRetry;
                    viewModel.MaxNumRetry = part.MaxNumRetry;
                    viewModel.MaxPushPerIteration = part.MaxPushPerIteration;
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts/PushMobileSettings_Edit", Model: viewModel, Prefix: Prefix);
            })
                .OnGroup("PushMobile");
        }

        protected override void Importing(PushMobileSettingsPart part, ImportContentContext context) {
            //           context.ImportAttribute(part.PartDefinition.Name, "DefaultParserEngine", x => part.DefaultParserIdSelected = x);
        }

        protected override void Exporting(PushMobileSettingsPart part, ExportContentContext context) {
            //           context.Element(part.PartDefinition.Name).SetAttributeValue("DefaultParserEngine", part.DefaultParserIdSelected);


        }

    }
}