using Laser.Orchard.Pdf.Models;
using Laser.Orchard.Pdf.ViewModels;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tokens;

namespace Laser.Orchard.Pdf.Drivers {
    public class PdfButtonPartDriver : ContentPartDriver<PdfButtonPart> {
        private readonly ITokenizer _tokenizer;
        protected override string Prefix {
            get { return "Laser.Orchard.Pdf"; }
        }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public PdfButtonPartDriver(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(PdfButtonPart part, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, shapeHelper);
        }
        protected override DriverResult Editor(PdfButtonPart part, dynamic shapeHelper) {
            var settings = part.Settings.GetModel<PdfButtonPartSettings>();
            string toParse = "";
            if (part.Settings.TryGetValue("PdfButtonPartSettings.PdfButtons", out toParse)) {
                settings.LoadStringToList(toParse);
            }
            var model = new PdfButtonPartVM { ContentId = part.Id, ButtonsSettings = settings };
            return ContentShape("Parts_PdfButtonPart", () => 
                shapeHelper.EditorTemplate(TemplateName: "Parts/PdfButtonPart", Model: model, Prefix: Prefix));
        }
    }
}