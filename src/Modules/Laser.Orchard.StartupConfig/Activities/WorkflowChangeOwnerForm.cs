using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.Activities {
    public class WorkflowChangeOwnerForm : IFormProvider {
        protected dynamic _shapeFactory { get; set; }
        private readonly IContentManager _contentManager;
        public Localizer T { get; set; }

        public WorkflowChangeOwnerForm(IShapeFactory shapeFactory, IContentManager contentManager) {
            _shapeFactory = shapeFactory;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {

            Func<IShapeFactory, dynamic> form =
              shape => {
                  var f = _shapeFactory.Form(
                      Id: "TheFormActivityChangeOwner",
                      _Type: _shapeFactory.FieldSet(
                          Title: T("Select the new Owner of ContentItem"),

                          _ddlOwner: _shapeFactory.SelectList(
                              Id: "allusers",
                              Name: "allusers",
                              title: T("all users"),
                              Description: T("select some user"),
                              Size: 10,
                              Multiple: false
                              )
                          //),
                          //  _filter: _shapeFactory.Check(
                          // Title: T("Set Owner of content picker element"),
                
                          //  Id: "operator-is-one-of", Name: "Operator",
                          //   Value: "0", Checked: true
                            )

                      );
                 // f._Type._ddlOwner.Add(new SelectListItem { Value = "", Text = T("Any").Text });
                  //  var userProfiles = _contentManager.Query<UserPart, UserPartRecord>().List().Select(user => ((dynamic)user).ProfilePart).Select(user => new { user.FirstName, user.LastName });
                  IEnumerable<UserPart> users = _contentManager
                   .Query<UserPart, UserPartRecord>()
                      .Where(x => x.UserName != null)
                      .List();

                  foreach (UserPart up in users) {
                      f._Type._ddlOwner.Add(new SelectListItem { Value = up.Id.ToString(), Text = up.UserName.ToString() });
                      //            f._ddlOwner.Add(new SelectListItem { Value = userProfiles[i].FirstName, Text = userProfiles[i].LastName });
                  }
                  //var singleuser in userProfiles.Count) {
                  //    f._ddlOwner.Add(new SelectListItem { Value = singleuser.FirstName, Text = singleuser.LastName });

                  //var submittedByField = ((dynamic)q.ContentItem).Question.SubmittedBy;
                  //sbmittedByField.Ids = new[] { submittedById };

                  return f;
              };
            context.Form("TheFormActivityChangeOwner", form);
        }
    }
}