using System;
using System.Linq;
using Orchard.Taxonomies.Fields;
using Orchard.Localization;
using Orchard.Tokens;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class TaxonomyTokens: ITokenProvider {

        public TaxonomyTokens() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            // Usage:
            // Content.Fields.Article.Categories.Terms -> 'Science, Sports, Arts'
            // Content.Fields.Article.Categories.Terms:0 -> 'Science'

            // When used with an indexer, it can be chained with Content tokens
            // Content.Fields.Article.Categories.Terms:0.DisplayUrl -> http://...

            context.For("TaxonomyField", T("Taxonomy Field"), T("Tokens for Taxonomy Fields"))
                   .Token("TermsIds", T("Terms"), T("The ids of the terms associated with field."))
                   ;
        }

        public void Evaluate(EvaluateContext context) {

            context.For<TaxonomyField>("TaxonomyField")
                    .Token("TermsIds", field => String.Join(", ", field.Terms.Select(t => t.Id).ToArray()))
                   // todo: extend Chain() in order to accept a filter like in Token() so that we can chain on an expression
                   .Chain("Terms:0", "Content", t => t.Terms.Count() > 0 ? t.Terms.ElementAt(0) : null)
                   .Chain("Terms:1", "Content", t => t.Terms.Count() > 1 ? t.Terms.ElementAt(1) : null)
                   .Chain("Terms:2", "Content", t => t.Terms.Count() > 2 ? t.Terms.ElementAt(2) : null)
                   .Chain("Terms:3", "Content", t => t.Terms.Count() > 3 ? t.Terms.ElementAt(3) : null)
                   ;
        }
    }
}