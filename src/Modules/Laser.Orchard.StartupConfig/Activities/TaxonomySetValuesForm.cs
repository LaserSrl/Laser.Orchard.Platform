using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.Activities {
    public class TaxonomySetValuesForm : IFormProvider {
        protected dynamic _shapeFactory { get; set; }
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;
        public Localizer T { get; set; }

        public TaxonomySetValuesForm(IShapeFactory shapeFactory, IContentManager contentManager, ITaxonomyService taxonomyService) {
            _shapeFactory = shapeFactory;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
            _taxonomyService = taxonomyService;
        }

        public void Describe(DescribeContext context) {

            Func<IShapeFactory, dynamic> form =
              shape => {
                  var f = _shapeFactory.Form(
                      Id: "TheFormTaxonomySetValues",
                      _Type: _shapeFactory.FieldSet(
                          Title: T("Select Terms of ContentItem"),

                          _ddlTerms: _shapeFactory.SelectList(
                              Id: "allterms",
                              Name: "allterms",
                              title: T("all Termss"),
                              Description: T("select a Term"),
                              Size: 10,
                              Multiple: true
                              )
                          //),
                          //  _filter: _shapeFactory.Check(
                          // Title: T("Set Owner of content picker element"),
                
                          //  Id: "operator-is-one-of", Name: "Operator",
                          //   Value: "0", Checked: true
                            )

                      );

                 IEnumerable<TaxonomyPart> listtaxonomies= _taxonomyService.GetTaxonomies();
                 //foreach (TaxonomyPart tp in listtaxonomies) {
                 //    f._Type._ddlTaxonomies.Add(new SelectListItem { Value = tp.Id.ToString(), Text = tp.Name.ToString() });

                 //}

                 
                 foreach (TaxonomyPart tp in listtaxonomies) {
                     f._Type._ddlTerms.Add(new SelectListItem { Value = tp.Id.ToString(), Text = tp.Name.ToString() });
                     IEnumerable<TermPart> listterms = _taxonomyService.GetTerms(tp.Id);
                     foreach (TermPart termp in listterms)
                         f._Type._ddlTerms.Add(new SelectListItem { Value = termp.Id.ToString(), Text = " - " + termp.Name.ToString() });

                 }
                  // f._Type._ddlOwner.Add(new SelectListItem { Value = "", Text = T("Any").Text });
                  //  var userProfiles = _contentManager.Query<UserPart, UserPartRecord>().List().Select(user => ((dynamic)user).ProfilePart).Select(user => new { user.FirstName, user.LastName });
                  //IEnumerable<UserPart> users = _contentManager
                  // .Query<UserPart, UserPartRecord>()
                  //    .Where(x => x.UserName != null)
                  //    .List();

                  //foreach (UserPart up in users) {
                  //    f._Type._ddlOwner.Add(new SelectListItem { Value = up.Id.ToString(), Text = up.UserName.ToString() });
                  //    //            f._ddlOwner.Add(new SelectListItem { Value = userProfiles[i].FirstName, Text = userProfiles[i].LastName });
                  //}
                  //var singleuser in userProfiles.Count) {
                  //    f._ddlOwner.Add(new SelectListItem { Value = singleuser.FirstName, Text = singleuser.LastName });

                  //var submittedByField = ((dynamic)q.ContentItem).Question.SubmittedBy;
                  //sbmittedByField.Ids = new[] { submittedById };

                  return f;
              };
            context.Form("TheFormTaxonomySetValues", form);
        }
    }
}