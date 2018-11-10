using System;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.Tokens;
using System.Text.RegularExpressions;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class StripHtmlToken : ITokenProvider {

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Text", T("Text"), T("Tokens for text strings"))
                .Token("StripHtml", T("Strip Html Text"), T("Strip an HTML string."), "Text")
                ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<String>("Text", () => "")
                .Token("StripHtml", i => Regex.Replace(i, "<.*?>", String.Empty))
                .Chain("StripHtml", "Text", i => Regex.Replace(i, "<.*?>", String.Empty))
                ;
        }
    }
}