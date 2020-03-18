using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
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
            get { return "Laser.Orchard.NwazetIntegration"; }
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
    }
}