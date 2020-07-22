using Laser.Orchard.ContactForm.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.ContactForm.Activity {
    public class ContactFormSubmittedForm : IFormProvider {
        protected dynamic _shapeFactory { get; set; }
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public Localizer T { get; set; }

        public ContactFormSubmittedForm(IShapeFactory shapeFactory, IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _shapeFactory = shapeFactory;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {

            Func<IShapeFactory, dynamic> form =
                 shape => {

                     var f = _shapeFactory.Form(
                         Id: "_AnyOfContentTypes",
                         _Contents: _shapeFactory.SelectList(
                             Id: "identityValues", Name: "IdentityValues",
                             Title: T("Contact Forms - Contents"),
                             Description: T("Select any contact form. If the contact form is not in the list, you need to add the IdentityPart and valorize it."),
                             Size: 10,
                             Multiple: true
                             )
                         );

                     f._Contents.Add(new SelectListItem { Value = "", Text = T("Any").Text });
                     var listOfContactForms = _contentManager.Query()
                        .ForPart<ContactFormPart>()
                        .Join<TitlePartRecord>()
                        .Join<IdentityPartRecord>().List()
                            .Select(x => {
                                if (string.IsNullOrWhiteSpace(x.As<IdentityPart>().Identifier)) {
                                    x.As<IdentityPart>().Identifier = Guid.NewGuid().ToString("n");
                                }

                                return new SelectListItem {
                                    Value = _contentManager.GetItemMetadata(x).Identity.ToString(),
                                    Text = x.As<TitlePart>().Title + " - " + x.ContentItem.ContentType
                                };
                            }).Where(x => x.Value != "").ToList(); // Create a list of contact form items
                     listOfContactForms.AddRange(_contentManager.Query()
                         .ForPart<ContactFormPart>()
                         .Join<WidgetPartRecord>()
                         .Join<IdentityPartRecord>().List()
                                .Select(x => {
                                    if (string.IsNullOrWhiteSpace(x.As<IdentityPart>().Identifier)) {
                                        x.As<IdentityPart>().Identifier = Guid.NewGuid().ToString("n");
                                    }

                                    return new SelectListItem {
                                        Value = _contentManager.GetItemMetadata(x).Identity.ToString(),
                                        Text = x.As<WidgetPart>().Title + " - " + x.ContentItem.ContentType
                                    };
                                }).Where(x => x.Value != "")); // Adds contact form widgets to list

                     foreach (var listItem in listOfContactForms.OrderBy(x => x.Text)) {
                         f._Contents.Add(listItem);
                     }

                     return f;
                 };

            context.Form("_ContactFormSubmittedForm", form);

        }
    }
}