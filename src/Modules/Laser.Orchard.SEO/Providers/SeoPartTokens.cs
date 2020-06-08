using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Tokens;

namespace Laser.Orchard.SEO.Providers {
    public class SeoPartTokens : ITokenProvider {

        public SeoPartTokens() {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content")
                .Token("SeoDescription", 
                    T("SEOPart.Description"), 
                    T("The description for the content that was input in its SEO Part."));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                // {Content.SeoDescription}
                .Token("SeoDescription", content => {
                    var seoPart = content.As<SeoPart>();
                    if (seoPart == null) {
                        return string.Empty;
                    }
                    return seoPart.Description;
                });
        }
    }
}