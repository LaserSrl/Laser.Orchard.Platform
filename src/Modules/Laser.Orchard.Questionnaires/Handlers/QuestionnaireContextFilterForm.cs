using Orchard.DisplayManagement;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OForms = Orchard.Forms;

namespace Laser.Orchard.Questionnaires.Handlers {
    public class QuestionnaireContextFilterForm : OForms.Services.IFormProvider {
        protected dynamic _shapeFactory { get; set; }
        public Localizer T { get; set; }
        public const string FormName = "QuestionnaireContextFilterForm";

        public QuestionnaireContextFilterForm(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
            T = NullLocalizer.Instance;
        }
        public void Describe(OForms.Services.DescribeContext context) {
            // compone il form
            context.Form(FormName, shape => {
                var f = _shapeFactory.Form(
                    Id: FormName,
                     _Context: _shapeFactory.FieldSet(
                            Id: "questionnaireContext",
                            _Value: _shapeFactory.TextBox(
                                Name: "Context",
                                Title: T("Questionnaire Context"),
                                Classes: new[] { "tokenized" }
                            )
                     )
                );
                return f;
            });
        }
    }
}