using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Services;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.TaxonomiesExtensions.Projections {
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
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
        public const string FormName = "SelectTermsForm";

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form = shape => {
                var f = Shape.Form(
                    Id: "SelectTermsForm",
                    _Terms: Shape.TextBox(
                        Id: "terms",
                        Name: "Terms",
                        Title: T("Term identifiers"),
                        Classes: new[] { "text medium tokenized" },
                        Description: T("If no valid term is provided, all content items will be included in the results. If some terms below have been selected, the term identifiers will be added without being duplicated.")),
                    _TermIds: Shape.SelectList(
                            Id: "termids", Name: "TermIds",
                            Title: T("Terms"),
                            Description: T("Select some terms. If term identifiers have been added above, the selected terms will be added without being duplicated."),
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
                    _IncludeChildren: Shape.Checkbox(
                        Id: "IncludeChildren", Name: "IncludeChildren",
                        Title: T("Automatically include children terms in filtering"),
                        Value: "true"
                    ));

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