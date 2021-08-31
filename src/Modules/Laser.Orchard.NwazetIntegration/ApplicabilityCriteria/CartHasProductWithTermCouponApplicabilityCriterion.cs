using Laser.Orchard.NwazetIntegration.Filters;
using Nwazet.Commerce.Descriptors.CouponApplicability;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services.Couponing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    public class CartHasProductWithTermCouponApplicabilityCriterion
         : BaseCouponCriterionProvider, ICouponApplicabilityCriterionProvider {

        private readonly ITaxonomyService _taxonomyService;

        public CartHasProductWithTermCouponApplicabilityCriterion(
          IWorkContextAccessor workContextAccessor,
          ICacheManager cacheManager,
          ISignals signals,
          ITaxonomyService taxonomyService)
          : base(workContextAccessor, cacheManager, signals) {

            _taxonomyService = taxonomyService;
        }

        public override string ProviderName => "CartHasProductWithTermCouponApplicabilityCriterion";

        public override LocalizedString ProviderDisplayName =>
         T("Criteria on product TaxonomyFields.");


        public void Describe(DescribeCouponApplicabilityContext describe) {
            var isAvailableForConfiguration = IsAvailableForConfiguration();
            var isAvailableForProcessing = IsAvailableForProcessing();

            describe
               .For("Cart", T("Cart products"), T("Cart products"))
               // Product MUST have selected term
               .Element("Cart has Products in the category",
                    T("Lines products are in the given category (if any was selected)"),
                    T("Lines products are in the given category (if any was selected)"),
                    (ctx) => ApplyCriterion(ctx, (b) => b),
                    (ctx) => ApplyCriterion(ctx, (b) => b),
                    (ctx) => DisplayTrueLabel(ctx),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    SelectTermsForm.FormName)
               // Product MUST not have selected term
               .Element("Cart has not Products in the category",
                    T("Lines products are not in the given category (if any was selected)"),
                    T("Lines products are not in the given category (if any was selected))"),
                    (ctx) => ApplyCriterion(ctx, (b) => !b),
                    (ctx) => ApplyCriterion(ctx, (b) => !b),
                    (ctx) => DisplayFalseLabel(ctx),
                    isAvailableForConfiguration, isAvailableForProcessing,
                    SelectTermsForm.FormName);
        }
        public LocalizedString DisplayFalseLabel(CouponContext ctx) {
            bool.TryParse(ctx.State.IncludeChildren?.Value, out bool includeChildren);
            if (includeChildren) {
                return T("Cart lines are not a products of the given category or one of its children.");
            }
            return T("Cart lines are not a products of the given category.");
        }

        public LocalizedString DisplayTrueLabel(CouponContext ctx) {
            bool.TryParse(ctx.State.IncludeChildren?.Value, out bool includeChildren);

            var opCart = (SelectTermsOperator)Enum.Parse(typeof(SelectTermsOperator), Convert.ToString(ctx.State.OperatorCart));

            switch (opCart) {
                case SelectTermsOperator.AllProducts:
                if (includeChildren) {
                    return T("Each lines are a products of the given category or one of its children.");
                }
                return T("Each lines are a products of the given category.");
                case SelectTermsOperator.OneProduct:
                if (includeChildren) {
                    return T("At least one lines are a products of the given category or one of its children.");
                }
                return T("At least one lines are a products of the given category.");
                case SelectTermsOperator.InsideCart:
                if (includeChildren) {
                    return T("Among products, there must be the given category or one of its children.");
                }
                return T("Among products, there must be the given category.");
                default:
                if (includeChildren) {
                    return T("Cart lines are a products of the given category or one of its children.");
                }
                return T("Cart lines are a products of the given category.");
            }
        }

        public void ApplyCriterion(CouponApplicabilityCriterionContext context,
            // Use outerCriterion to negate the test, so we can easily do
            // contains / doesn't contain
            Func<bool, bool> outerCriterion) {

            if (context.IsApplicable) {
                // If there is no configured term, this delegate will not affect the
                // applicability for the coupon in any way.
                // get from state the comma separated list of term ids
                List<int> allTermIds = new List<int>();
                var termsString = (string)context.State.Terms;
                if (!string.IsNullOrWhiteSpace(termsString)) {
                    allTermIds = termsString
                     .Split(new char[] { ',' })
                     .Select(int.Parse)
                     .ToList();
                }
                string selectedMultipleTerms = Convert.ToString(context.State.TermIds);
                if (!string.IsNullOrEmpty(selectedMultipleTerms)) {
                    allTermIds = allTermIds.Union(selectedMultipleTerms
                        .Split(new[] { ',' })
                        .Select(Int32.Parse)
                        .ToList()).ToList();
                }

                // check the list of ids for taxonomies
                // if there are, select all term ids
                allTermIds = allTermIds.Union(allTermIds
                    .Where(x => _taxonomyService.GetTaxonomy(x) != null)
                    .SelectMany(t => _taxonomyService.GetTerms(t).Select(term => term.Id))
                    .ToList()).ToList();

                if (allTermIds.Any()) {
                    bool.TryParse(context.State.IncludeChildren?.Value, out bool includeChildren);
                    var terms = allTermIds
                        .SelectMany(id => GetTerms(id, includeChildren))
                        .Distinct();
                    // check again, in case the ids do not match any term
                    if (terms.Any()) {
                        var result = false;

                        // get product in shopping cart
                        var products = context
                          .ApplicabilityContext
                          .ShoppingCart
                          .GetProducts();

                        // "is one of" or "is all of"
                        int op = Convert.ToInt32(context.State.Operator);

                        var opCart = (SelectTermsOperator)Enum.Parse(typeof(SelectTermsOperator), Convert.ToString(context.State.OperatorCart));

                        List<ShoppingCartQuantityProduct> checkedProductsList = new List<ShoppingCartQuantityProduct>();
                        switch (opCart) {
                            case SelectTermsOperator.AllProducts:
                            checkedProductsList = GetProductByTerm(terms, products, op);
                            if (products.Count() == checkedProductsList.Count()) {
                                result = true;
                            }
                            break;
                            case SelectTermsOperator.OneProduct:
                            checkedProductsList = GetProductByTerm(terms, products, op);
                            result = checkedProductsList.Any();
                            break;
                            case SelectTermsOperator.InsideCart:
                            var termsPart = products
                                 .Where(p => p.Product.As<TermsPart>() != null)
                                 .SelectMany(p => p.Product.As<TermsPart>().TermParts);

                            // if no term is selected in the product, we already know this will be false
                            if (termsPart.Any()) {
                                var selectedTerms = termsPart.Select(p => p.TermPart);

                                switch (op) {
                                    case 0: // is one of
                                    result = terms.Any(t => selectedTerms.Contains(t));
                                    break;
                                    case 1: // is all of
                                    result = terms.All(t => selectedTerms.Contains(t));
                                    break;
                                }
                            }
                            break;
                        }
                        result = outerCriterion(result);

                        context.ApplicabilityContext.IsApplicable = result;
                        context.IsApplicable = result;
                    }
                }
            }
        }

        private IEnumerable<TermPart> GetTerms(int termId, bool andChildren) {
            var term = _taxonomyService.GetTerm(termId);
            if (term == null) {
                return Enumerable.Empty<TermPart>();
            }
            var ts = new List<TermPart>() { term };
            if (andChildren) {
                ts.AddRange(_taxonomyService.GetChildren(term));
            }
            return ts;
        }

        private List<ShoppingCartQuantityProduct> GetProductByTerm(
                IEnumerable<TermPart> terms,
                IEnumerable<ShoppingCartQuantityProduct> products,
                int op) {

            var productsList = new List<ShoppingCartQuantityProduct>();
            foreach (var p in products) {
                if (p.Product.As<TermsPart>() != null) {
                    var selectedTerms = p.Product.As<TermsPart>().TermParts.Select(tcip => tcip.TermPart);
                    switch (op) {
                        case 0: // is one of
                        if (terms.Any(t => selectedTerms.Contains(t))) {
                            productsList.Add(p);
                        }
                        break;
                        case 1: // is all of
                        if (terms.All(t => selectedTerms.Contains(t))) {
                            productsList.Add(p);
                        }
                        break;
                    }
                }
            }

            return productsList;
        }

    }
}