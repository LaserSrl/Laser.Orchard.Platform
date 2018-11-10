namespace Proligence.QrCodes.Handlers {
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using Gma.QrCodeNet.Encoding;
    using Gma.QrCodeNet.Encoding.Windows.Render;
    using Orchard;
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Handlers;
    using Orchard.Data;
    using Orchard.Environment.Configuration;
    using Orchard.Tokens;

    using Proligence.QrCodes.Models;
    using Proligence.QrCodes.Settings;

    public class QrCodePartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly ITokenizer _tokenizer;
        public QrCodePartHandler(
            IRepository<QrCodePartRecord> repository, 
            IContentManager contentManager, 
            ITokenizer tokenizer, 
            IOrchardServices orchardServices, 
            ShellSettings shellSettings) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
            _tokenizer = tokenizer;
            this.Filters.Add(StorageFilter.For(repository));
            this.OnLoaded<QrCodePart>((ctx, part) => {
                var settings = part.Settings.GetModel<QrCodeTypePartSettings>();
                part.Size = part.Record.Size == default(int) ? settings.Size : part.Record.Size;
                part.Value = string.IsNullOrWhiteSpace(part.Record.Value) ? settings.Value : part.Record.Value;
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                part.ActualValue = _tokenizer.Replace(part.Value, tokens);
            });
            this.OnUpdated<QrCodePart>((ctx, part) => {
                CreateFile(part);
            });
            this.OnUnpublished<QrCodePart>((ctx, part) => {
                CreateFile(part);
            });
        }
        private void CreateFile(QrCodePart part) {
            var item = _contentManager.Get(part.ContentItem.Id);
            if (item != null && item.Has<QrCodePart>()) {
                var qrEncoder = new QrEncoder(ErrorCorrectionLevel.M);
                var qrCode = new QrCode();
               var thevalue=string.IsNullOrWhiteSpace(part.Record.Value) ? part.Settings.GetModel<QrCodeTypePartSettings>().Value : part.Record.Value;
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                thevalue = _tokenizer.Replace(thevalue, tokens);
                if (qrEncoder.TryEncode(thevalue, out qrCode)) {
                    var renderer = new GraphicsRenderer(new FixedCodeSize(item.As<QrCodePart>().Size, QuietZoneModules.Zero), Brushes.Black, Brushes.White);
                    var stream = new MemoryStream();
                    renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);
                    stream.Position = 0;
                    var outputPath = _orchardServices.WorkContext.HttpContext.Server.MapPath(string.Format("~/Media/{0}/qrcode", _shellSettings.Name));
                    if (!Directory.Exists(outputPath))
                        Directory.CreateDirectory(outputPath);
                    var filename = outputPath + @"\qrcode_" + part.ContentItem.Id.ToString() + ".png";
                    using (FileStream file = new FileStream(filename, FileMode.Create, System.IO.FileAccess.Write)) {
                        var bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, (int)stream.Length);
                        file.Write(bytes, 0, bytes.Length);
                        stream.Close();
                    }
                }
            }
        }
    }
}
