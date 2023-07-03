using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Laser.Orchard.SEO.Services;
using Orchard;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;
using Orchard.Tokens;
using Orchard.UI.Resources;


namespace Laser.Orchard.SEO.Shapes {


    [OrchardFeature("Laser.Orchard.Favicon")]
    public class FaviconShapes : IShapeTableProvider {


        private readonly IWorkContextAccessor _wca;
        private readonly IFaviconService _faviconService;
        private readonly ITokenizer _tokenizer;

        public FaviconShapes(IWorkContextAccessor wca, IFaviconService faviconService, ITokenizer tokenizer) {
            _wca = wca;
            _faviconService = faviconService;
            _tokenizer = tokenizer;
        }


        public void Discover(ShapeTableBuilder builder) {

            builder.Describe("HeadLinks")
                .OnDisplaying(shapeDisplayingContext => {

                    var faviconUrl = _faviconService.GetFaviconUrl();

                    if (!string.IsNullOrWhiteSpace(faviconUrl)) {
                        if (faviconUrl.Contains("{") || faviconUrl.Contains("#{")) {
                            Dictionary<string, object> tokens = new Dictionary<string, object>();
                            faviconUrl = _tokenizer.Replace(faviconUrl, tokens);
                        }
                        // Get the current favicon from head
                        var resourceManager = _wca.GetContext().Resolve<IResourceManager>();
                        var links = resourceManager.GetRegisteredLinks();
                        var currentFavicon = links.FirstOrDefault(l => l.Rel == "shortcut icon" && l.Type == "image/x-icon");

                        // Modify if found
                        if (currentFavicon != default(LinkEntry)) {
                            currentFavicon.Href = faviconUrl;
                        } else {
                            // Add the new one
                            resourceManager.RegisterLink(new LinkEntry {
                                Type = "image/x-icon",
                                Rel = "shortcut icon",
                                Href = faviconUrl
                            });
                        }
                    }
                });
        }


    }
}