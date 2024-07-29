using Laser.Orchard.Questionnaires.Models;
using NHibernate.Util;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Security;
using System.Linq;

namespace Laser.Orchard.Questionnaires.Drivers {
    public class QuestionnaireSpecificAccessPartDriver : ContentPartDriver<QuestionnaireSpecificAccessPart> {
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;

        protected override string Prefix {
            get {
                return "QuestionnaireSpecificAccess";
            }
        }
        public Localizer T { get; set; }

        public QuestionnaireSpecificAccessPartDriver(IAuthorizer authorizer,
            IContentManager contentManager) {

            _authorizer = authorizer;
            _contentManager = contentManager;

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

            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("QuestionnaireUpdateError", T("Cannot update questionnaire"));
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(QuestionnaireSpecificAccessPart part, ExportContentContext context) {
            if (part.UserIds.Any()) {
                var identities = part.UserIds
                    .Select(id => _contentManager.Get(id))
                    .Where(ci => ci != null)
                    .Select(ci => _contentManager.GetItemMetadata(ci).Identity.ToString())
                    .ToArray();

                context.Element(part.PartDefinition.Name)
                    .SetAttributeValue("UserIds", string.Join(",", identities));
            }
        }

        protected override void Importing(QuestionnaireSpecificAccessPart part, ImportContentContext context) {
            var element = context.Data.Element(part.PartDefinition.Name);
            if (element != null) {
                var identities = context.Attribute(part.PartDefinition.Name, "UserIds");
                if (identities != null) {
                    part.UserIds = identities.Split(',')
                        .Select(id => context.GetItemFromSession(id))
                        .Select(ci => ci.Id)
                        .ToList();
                } else {
                    part.UserIds = new int[0];
                }
            }
        }
    }
}