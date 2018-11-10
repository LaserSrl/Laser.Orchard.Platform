using Laser.Orchard.Pdf.Models;
using Laser.Orchard.Pdf.Services;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Core.Contents;
using Orchard.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;

namespace Laser.Orchard.Pdf.Controllers {
    public class PdfController : Controller {
        private readonly IPdfServices _pdfServices;
        private readonly ITemplateService _templateService;
        private readonly IOrchardServices _orchardServices;
        private readonly ITokenizer _tokenizer;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public PdfController(IPdfServices pdfServices, ITemplateService templateService, IOrchardServices orchardServices, ITokenizer tokenizer) {
            _pdfServices = pdfServices;
            _tokenizer = tokenizer;
            _templateService = templateService;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        private ActionResult GeneratePdf(ContentItem ci, int pid) {
            if (ci != null) {
                if (_orchardServices.Authorizer.Authorize(Permissions.ViewContent, ci)) {
                   var part = ci.Parts.FirstOrDefault(x => x.PartDefinition.Name == typeof(PdfButtonPart).Name);
                    if (part != null) {
                        var settings = part.Settings.GetModel<PdfButtonPartSettings>();
                        string toParse = "";
                        if (part.Settings.TryGetValue("PdfButtonPartSettings.PdfButtons", out toParse)) {
                            settings.LoadStringToList(toParse);
                        }
                        var buttonSettings = settings.PdfButtons.First(x => x.Position == pid);
                        var html = _templateService.RitornaParsingTemplate(ci, buttonSettings.TemplateId);
                        var editModel = new Dictionary<string, object>();
                        editModel.Add("Content", ci);
                        var header = _tokenizer.Replace(buttonSettings.Header, editModel);
                        var footer = _tokenizer.Replace(buttonSettings.Footer, editModel);
                        byte[] buffer = null;
                        if (string.IsNullOrWhiteSpace(header) && string.IsNullOrWhiteSpace(footer)) {
                            buffer = _pdfServices.PdfFromHtml(html, buttonSettings.PageWidth, buttonSettings.PageHeight, buttonSettings.LeftMargin, buttonSettings.RightMargin, buttonSettings.HeaderHeight, buttonSettings.FooterHeight);
                        }
                        else {
                            var headerFooter = _pdfServices.GetHtmlHeaderFooterPageEvent(HttpUtility.HtmlDecode(header), HttpUtility.HtmlDecode(footer));
                            buffer = _pdfServices.PdfFromHtml(html, buttonSettings.PageWidth, buttonSettings.PageHeight, buttonSettings.LeftMargin, buttonSettings.RightMargin, buttonSettings.HeaderHeight, buttonSettings.FooterHeight, headerFooter);
                        }
                        var fileName = _tokenizer.Replace(buttonSettings.FileNameWithoutExtension, editModel);
                        fileName = string.Format("{0}.pdf", (string.IsNullOrWhiteSpace(fileName) ? "page" : fileName.Trim()));
                        return File(buffer, "application/pdf", fileName);
                    }
                }
                else {
                    return new HttpUnauthorizedResult();
                }
            }
            // fallback
            var htmlError = "Please save your content to generate PDF.";
            return Content(htmlError, "text/html", Encoding.UTF8);
        }


        public ActionResult Generate(int id, int pid) {
            var ci = _orchardServices.ContentManager.Get(id, VersionOptions.Published);
            return GeneratePdf(ci, pid);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Preview(int id, int pid) {
            var ci = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
            return GeneratePdf(ci, pid);
        }

        //public ActionResult Preview(int id) {
        //    ContentPart part = null;
        //    var ci = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
        //    if (ci != null) {
        //        if (_orchardServices.Authorizer.Authorize(Permissions.ViewContent, ci)) {
        //            part = ci.Parts.FirstOrDefault(x => x.PartDefinition.Name == typeof(PdfButtonPart).Name);
        //            if (part != null) {
        //                var settings = part.Settings.GetModel<PdfButtonPartSettings>();
        //                var html = _templateService.RitornaParsingTemplate(ci, settings.TemplateId);
        //                return Content(html, "text/html", Encoding.UTF8);
        //            }
        //        }
        //        else {
        //            return new HttpUnauthorizedResult();
        //        }
        //    }
        //    // fallback
        //    var htmlError = "Please save your content to see the preview.";
        //    return Content(htmlError, "text/html", Encoding.UTF8);
        //}
    }
}