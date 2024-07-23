using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Security;

namespace Laser.Orchard.Questionnaires.Drivers {
    public class QuestionnaireSpecificAccessPartDriver : ContentPartDriver<QuestionnaireSpecificAccessPart> {
        private readonly IAuthorizer _authorizer;

        protected override string Prefix {
            get {
                return "QuestionnaireSpecificAccess";
            }
        }
        public Localizer T { get; set; }

        public QuestionnaireSpecificAccessPartDriver(IAuthorizer authorizer) {
            _authorizer = authorizer;

            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(QuestionnaireSpecificAccessPart part, dynamic shapeHelper) {
            if (!_authorizer.Authorize(Permissions.ManageAccessToSpecificQuestionnaireStatistics)) {
                return ContentShape("Parts_QuestionnaireSpecificAccessPart_Edit",
                   () => shapeHelper.EditorTemplate(TemplateName: "Parts/EmptyShape"));
            }
            return ContentShape("Parts_QuestionnaireSpecificAccessPart_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/QuestionnaireSpecificAccessPart_Edit",
                Model: part,
                Prefix: Prefix));

        }

        protected override DriverResult Editor(QuestionnaireSpecificAccessPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizer.Authorize(Permissions.ManageAccessToSpecificQuestionnaireStatistics)) {
                return ContentShape("Parts_QuestionnaireSpecificAccessPart_Edit",
                   () => shapeHelper.EditorTemplate(TemplateName: "Parts/EmptyShape"));
            }

            if (updater.TryUpdateModel(part, Prefix, null, null)) {
                //part.Record.SerializedUserIds = part.Record.EncodeIds(part.UserIds);
            } else {
                updater.AddModelError("QuestionnaireUpdateError", T("Cannot update questionnaire"));
            }

            return Editor(part, shapeHelper);
        }
    }
}