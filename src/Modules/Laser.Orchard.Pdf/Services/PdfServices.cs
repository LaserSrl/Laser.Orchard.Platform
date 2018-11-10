using Orchard;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.IO;
using Orchard.Logging;
using Orchard.Localization;
using Laser.Orchard.Pdf.Services.PageEvents;
using System.Text;
using System;

namespace Laser.Orchard.Pdf.Services {
    public interface IPdfServices : IDependency {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="html">HTML content to populate pdf document.</param>
        /// <param name="pageSize">Size of pdf page. Valid values: A5, A4, A3, A2, A1, A0. Default: "A4"</param>
        /// <param name="marginLeft">Default 50.</param>
        /// <param name="marginRight">Default 50.</param>
        /// <param name="marginTop">Default 10.</param>
        /// <param name="marginBottom">Default 10.</param>
        /// <param name="landscape">Default false.</param>
        /// <returns></returns>
        byte[] PdfFromHtml(string html, string pageSize = "A4", float marginLeft = 50, float marginRight = 50, float marginTop = 10, float marginBottom = 10, bool landscape = false, IPdfPageEvent pdfPageEvent = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="html">HTML content to populate pdf document.</param>
        /// <param name="width">Page width.</param>
        /// <param name="height">Page height.</param>
        /// <param name="marginLeft">Default 50.</param>
        /// <param name="marginRight">Default 50.</param>
        /// <param name="marginTop">Default 10.</param>
        /// <param name="marginBottom">Default 10.</param>
        /// <returns></returns>
        byte[] PdfFromHtml(string html, float width, float height, float marginLeft = 50, float marginRight = 50, float marginTop = 10, float marginBottom = 10, IPdfPageEvent pdfPageEvent = null);
        IPdfPageEvent GetHtmlHeaderFooterPageEvent(string header, string footer);
    }
    public class PdfServices : IPdfServices {
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public PdfServices() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public byte[] PdfFromHtml(string html, string pageSize = "A4", float marginLeft = 50, float marginRight = 50, float marginTop = 10, float marginBottom = 10, bool landscape = false, IPdfPageEvent pdfPageEvent = null) {
            var effectivePageSize = PageSize.A4;
            switch (pageSize.ToUpper()) {
                case "A5":
                    effectivePageSize = PageSize.A5;
                    break;
                case "A4":
                    effectivePageSize = PageSize.A4;
                    break;
                case "A3":
                    effectivePageSize = PageSize.A3;
                    break;
                case "A2":
                    effectivePageSize = PageSize.A2;
                    break;
                case "A1":
                    effectivePageSize = PageSize.A1;
                    break;
                case "A0":
                    effectivePageSize = PageSize.A0;
                    break;
            }
            if (landscape) {
                effectivePageSize = effectivePageSize.Rotate();
            }
            return PdfFromHtml(html, effectivePageSize.Width, effectivePageSize.Height, marginLeft, marginRight, marginTop, marginBottom, pdfPageEvent);
        }
        public byte[] PdfFromHtml(string html, float width, float height, float marginLeft = 50, float marginRight = 50, float marginTop = 10, float marginBottom = 10, IPdfPageEvent pdfPageEvent = null) {
            var effectivePageSize = new Rectangle(width, height);
            byte[] buffer = null;
            using (var memoryStream = new MemoryStream()) {
                using (var document = new Document(effectivePageSize, marginLeft, marginRight, marginTop, marginBottom)) {
                    using (var writer = PdfWriter.GetInstance(document, memoryStream)) {
                        writer.PageEvent = pdfPageEvent;
                        int totalfonts = FontFactory.RegisterDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
                        using (var sr = new StringReader(html)) {
                            document.Open();
                            XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, sr);
                            document.Close();
                            buffer = memoryStream.ToArray();
                        }
                    }
                }
            }
            return buffer;
        }
        public IPdfPageEvent GetHtmlHeaderFooterPageEvent(string header, string footer) {
            return new HtmlHeaderFooter(header, footer);
        }
    }
}