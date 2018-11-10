using System;
using System.Xml.Linq;
using Laser.Orchard.Maps.Models;
using Laser.Orchard.Maps.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Orchard.Tokens;
using System.Collections.Generic;
using System.Net;
using Orchard.Environment.Configuration;
using System.Web;

namespace Laser.Orchard.Maps.Drivers {

    public class CustomPinPartDriver : ContentPartCloningDriver<CustomPinPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly ITokenizer _tokenizer;
        private readonly ShellSettings _shellSettings;
        private string _defaultPinUri;
        protected override string Prefix {
            get { return "Maps.CustomPin"; }
        }
        public CustomPinPartDriver(IOrchardServices orchardServices, ITokenizer tokenizer, ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _shellSettings = shellSettings;
            _tokenizer = tokenizer;
            _defaultPinUri = "~/Modules/Laser.Orchard.Maps/Contents/default-pin.png";
            //// test per cambio culture
            //System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;  // #GM 2015-09-15
        }
        public ILogger Logger { get; set; }

        public Localizer T { get; set; }

        //GET
        protected override DriverResult Editor(CustomPinPart part, dynamic shapeHelper) {
            return ContentShape("Parts_CustomPin_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/CustomPin_Edit",
                                    Model: part,
                                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(CustomPinPart part, IUpdateModel updater, dynamic shapeHelper) {
            var settings = part.Settings.GetModel<CustomPinPartSettings>();
            var siteSettings = _shellSettings.Name;
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            var pinUri = _defaultPinUri;
            var waterMarkUri = _tokenizer.Replace(settings.WaterMarkUrl, tokens);
            if (!string.IsNullOrWhiteSpace(settings.PinUrl)) {
                pinUri = _tokenizer.Replace(settings.PinUrl, tokens);
            }
            using (Image image = StringToImage(pinUri))
            using (Graphics g = Graphics.FromImage(image)) {
                ImageAttributes imageAttributes = new ImageAttributes();
                int width = image.Width;
                int height = image.Height;
                var oldColor = _tokenizer.Replace(settings.ReplacedColor, tokens);
                var newColor = _tokenizer.Replace(settings.AlternateColor, tokens);
                if (!string.IsNullOrWhiteSpace(newColor)) {
                    ColorMap colorMap = new ColorMap();
                    colorMap.OldColor = HexToColor(oldColor);
                    colorMap.NewColor = HexToColor(newColor);

                    ColorMap[] remapTable = { colorMap };

                    imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);
                    g.DrawImage(image, 0, 0, width, height);

                    g.DrawImage(
                       image,
                       new Rectangle(0, 0, width, height),  // destination rectangle 
                       0, 0,        // upper-left corner of source rectangle 
                       width,       // width of source rectangle
                       height,      // height of source rectangle
                       GraphicsUnit.Pixel,
                       imageAttributes);
                }


                // WaterMark
                if (!string.IsNullOrWhiteSpace(waterMarkUri)) {
                    using (Image watermarkImage = StringToImage(waterMarkUri)) {// Create a tokenized Settings for these
                        int centerX = 300, centerY = 300, drawableWidth = 280, drawableHeight = 280; // Create Settings for these
                        if (pinUri != _defaultPinUri) {
                            int.TryParse(_tokenizer.Replace(settings.TargetArea.CenterX, tokens), out centerX);
                            int.TryParse(_tokenizer.Replace(settings.TargetArea.CenterY, tokens), out centerY);
                            int.TryParse(_tokenizer.Replace(settings.TargetArea.DrawableWidth, tokens), out drawableWidth);
                            int.TryParse(_tokenizer.Replace(settings.TargetArea.DrawableHeight, tokens), out drawableHeight);
                        }
                        float scale = Math.Min((float)drawableWidth / watermarkImage.Width, (float)drawableHeight / watermarkImage.Height);
                        int newWidth = Convert.ToInt32(watermarkImage.Width * scale);
                        int newHeight = Convert.ToInt32(watermarkImage.Height * scale);
                        int x = centerX - (newWidth / 2);
                        int y = centerY - (newHeight / 2);

                        var resizedWatermarkImage = new Bitmap(watermarkImage, newWidth, newHeight);

                        using (TextureBrush watermarkBrush = new TextureBrush(resizedWatermarkImage)) {
                            watermarkBrush.TranslateTransform(x, y);
                            g.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(newWidth, newHeight)));
                        }
                    }
                }
                var outputPath = _orchardServices.WorkContext.HttpContext.Server.MapPath(string.Format("~/Media/{0}/_mapspins/{1}", _shellSettings.Name, part.ContentItem.ContentType.ToLower()));
                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                image.Save(outputPath + @"\pin-" + part.Id + ".png");
            }
            return Editor(part, shapeHelper);
        }

        #region [ Import/Export ]
        protected override void Exporting(CustomPinPart part, ExportContentContext context) {

        }

        protected override void Importing(CustomPinPart part, ImportContentContext context) {

        }
        #endregion

        protected override void Cloning(CustomPinPart originalPart, CustomPinPart clonePart, CloneContentContext context) {

        }

        private Color HexToColor(string color) {
            int argb = int.Parse("FF000000", NumberStyles.HexNumber); // default is black
            int.TryParse(color.Replace("#", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out argb);
            Color clr = Color.FromArgb(argb);
            return clr;
        }

        private Image StringToImage(string uri) {
            try {
                if (uri.StartsWith("http", true, CultureInfo.InvariantCulture)) {
                    WebClient wc = new WebClient();
                    byte[] bytes = wc.DownloadData(HttpUtility.UrlDecode(uri));
                    MemoryStream ms = new MemoryStream(bytes);
                    return Image.FromStream(ms);
                } else if (uri.Contains("/")) {
                    return new Bitmap(_orchardServices.WorkContext.HttpContext.Server.MapPath(HttpUtility.UrlDecode(uri)));
                } else if (uri.Contains(@"\")) {
                    return new Bitmap(uri);
                }
            } catch (Exception ex) {
                Logger.Error(string.Format("{0}, uri:{1}", ex.Message, uri));
            }
            return null;
        }
    }
}