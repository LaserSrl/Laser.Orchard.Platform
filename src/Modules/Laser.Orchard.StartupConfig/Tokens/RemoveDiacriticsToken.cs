using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Globalization;
using System.Text;

namespace Laser.Orchard.StartupConfig.Tokens {
    /// <summary>
    /// Clean a text of the accents.
    /// E.g.: "identità" becomes "identita", "il cane è bianco" becomes "il cane e bianco".
    /// </summary>
    public class RemoveDiacriticsToken : ITokenProvider {
        public RemoveDiacriticsToken() {
            T = NullLocalizer.Instance;
        }

        public Localizer T {  get; set; }

        public void Describe(DescribeContext context) {
            context.For("Text", T("Text"), T("Tokens for text strings"))
                .Token("RemoveDiacritics",
                    T("RemoveDiacritics"),
                    T("Removes the diacritics (e.g. accents) from a text"));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<String>("Text", () => "")
                .Token(
                    token => token == String.Empty ? String.Empty : null,
                    (token, d) => d)
                .Token("RemoveDiacritics", text => RemoveDiacritics(text))
                .Chain("RemoveDiacritics", "Text", text => RemoveDiacritics(text));
        }

        private static string RemoveDiacritics(string text) {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++) {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}