using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tokens;

namespace Laser.Orchard.NwazetIntegration.Drivers {
    public class GTMProductDriver : ContentPartCloningDriver<GTMProductPart> {
        private readonly ITokenizer _tokenizer;
        private readonly IOrchardServices _orchardServices;
        private readonly IGTMProductService _GTMProductService;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        private readonly IContentManager _contentManager;

        protected override string Prefix {
            get { return "GTMProductPart"; }
        }


        public GTMProductDriver(
            IOrchardServices orchardServices, 
            ITokenizer tokenizer, 
            IContentManager contentManager,
            IGTMProductService GTMProductService) {

            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _tokenizer = tokenizer;
            _contentManager = contentManager;
            _GTMProductService = GTMProductService;
        }

        protected override DriverResult Display(GTMProductPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Product_TagManager", shape => {
                _GTMProductService.FillPart(part);
                var gtmProductVM = new GTMProductVM(part);
                return shapeHelper.Parts_Product_TagManager(
                    GTMProductVM: gtmProductVM,
                    DisplayType: displayType);
            });
        }
    }
}