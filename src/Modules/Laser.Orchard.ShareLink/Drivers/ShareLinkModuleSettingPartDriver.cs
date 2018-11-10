using Laser.Orchard.ShareLink.Models;
using Laser.Orchard.ShareLink.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.ShareLink.Drivers {

    public class ShareLinkModuleSettingPartDriver : ContentPartDriver<ShareLinkModuleSettingPart> {
        private readonly IOrchardServices _orchardServices;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.ShareLink"; }
        }

        public ShareLinkModuleSettingPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(ShareLinkModuleSettingPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ShareLinkModuleSettingPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_ShareLinkModuleSetting", () => {
                var viewModel = new ShareLinkModuleSettingVM();
                var getpart = _orchardServices.WorkContext.CurrentSite.As<ShareLinkModuleSettingPart>();
                viewModel.SharedBody = getpart.SharedBody;
                viewModel.SharedText = getpart.SharedText;
                viewModel.SharedLink = getpart.SharedLink;
                viewModel.SharedImage = getpart.SharedImage;
                viewModel.Fb_App = getpart.Fb_App;
                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        part.SharedBody = viewModel.SharedBody;
                        part.SharedText = viewModel.SharedText;
                        part.SharedLink = viewModel.SharedLink;
                        part.SharedImage = viewModel.SharedImage;
                        part.Fb_App = viewModel.Fb_App;
                    }
                }
                else {
                    viewModel.SharedText = part.SharedText;
                    viewModel.SharedLink = part.SharedLink;
                    viewModel.SharedImage = part.SharedImage;
                    viewModel.Fb_App = part.Fb_App;
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts/ShareLinkModuleSetting", Model: viewModel, Prefix: Prefix);
            })
             .OnGroup("ShareLink");
        }
    }
}