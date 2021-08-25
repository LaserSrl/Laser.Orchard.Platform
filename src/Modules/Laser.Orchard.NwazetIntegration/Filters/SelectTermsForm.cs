using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Services;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.Filters {
    public class SelectTermsForm : IFormProvider {
        private readonly ITaxonomyService _taxonomyService;
        protected dynamic Shape { get; set; }

        public SelectTermsForm(
            IShapeFactory shapeFactory,
            ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public const string FormName = "SelectTermsFormCart";

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form = shape => {
                var f = Shape.Form(
                    Id: "SelectTermsForm",
                    _Terms: Shape.TextBox(
                        Id: "terms",
                        Name: "Terms",
                        Title: T("Term identifiers"),
                        Classes: new[] { "text medium tokenized" },
                        Description: T("If no valid term is provided, all content items will be included in the results. If some terms have been selected, the term identifiers will be added without being duplicated.")),
                    _TermIds: Shape.SelectList(
                            Id: "termids", Name: "TermIds",
                            Title: T("Terms"),
                            Description: T("Select some terms. If term identifiers have been added, the selected terms will be added without being duplicated."),
                            Size: 10,
                            Multiple: true
                            ),
                    _Exclusion: Shape.FieldSet(
                        _OperatorOneOf: Shape.Radio(
                            Id: "operator-is-one-of", Name: "Operator",
                            Title: T("Is associated to one of the specified terms"), Value: "0", Checked: true
                            ),
                        _OperatorIsAllOf: Shape.Radio(
                            Id: "operator-is-all-of", Name: "Operator",
                            Title: T("Is associated to all the specified terms"), Value: "1"
                            )
                        ),
                    _FieldIncludeChildren: Shape.FieldSet(
                        Id: "fieldset-include-children",
                        _IncludeChildren: Shape.Checkbox(
                            Id: "IncludeChildren", Name: "IncludeChildren",
                            Title: T("Automatically include children terms in filtering"),
                            Value: "true"
                        )
                    ),
                    _OperatorCart: Shape.SelectList(
                            Id: "operator", Name: "OperatorCart",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                        )
                        .Add(new SelectListItem {
                            Value = Convert.ToString(SelectTermsOperator.AllProducts),
                            Text = T("Each product in the cart should have the selected condition").Text
                        })
                        .Add(new SelectListItem {
                            Value = Convert.ToString(SelectTermsOperator.OneProduct),
                            Text = T("At least one product in the cart should have the selected condition").Text
                        })
                        .Add(new SelectListItem {
                            Value = Convert.ToString(SelectTermsOperator.InsideCart),
                            Text = T("Among all the products in the cart, the selected condition must occur").Text
                        })
                    );

                foreach (var taxonomy in _taxonomyService.GetTaxonomies()) {
                    f._TermIds.Add(new SelectListItem { Value = String.Empty, Text = taxonomy.Name });
                    foreach (var term in _taxonomyService.GetTerms(taxonomy.Id)) {
                        var gap = new string('-', term.GetLevels());

                        if (gap.Length > 0) {
                            gap += " ";
                        }

                        f._TermIds.Add(new SelectListItem { Value = term.Id.ToString(), Text = gap + term.Name });
                    }
                }

                return f;
            };

            context.Form(FormName, form);
        }
    }
}

public enum SelectTermsOperator {
    AllProducts,
    OneProduct,
    InsideCart
}