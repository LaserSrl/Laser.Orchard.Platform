using Laser.Orchard.Questionnaires.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Tokens.Providers;
using System.Linq;
using Contents = Orchard.Core.Contents; //need this because Permissions class for Questionnaires is called like the default one

namespace Laser.Orchard.Questionnaires.Handlers {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {

        private readonly IAuthorizer _authorizer;
        private readonly IOrchardServices _orchardServices;

        public AuthorizationEventHandler(IAuthorizer authorizer,
            IOrchardServices orchardServices) {

            _authorizer = authorizer;
            _orchardServices = orchardServices;
        }
        public void Adjust(CheckAccessContext context) {
            if (!context.Granted &&
                context.Content.Is<QuestionnairePart>()) {
                //If we are trying to view a Questionnaire, check for the correct permission
                if (context.Permission.Name == Contents.Permissions.ViewContent.Name) {
                    context.Adjusted = true;
                    context.Permission = Permissions.SubmitQuestionnaire; //will check this permission next
                } else if (context.Content.Has<QuestionnaireSpecificAccessPart>()) {
                    // Check for the permission to access to a specific questionnaire for current user
                    if (context.Permission.Name == Permissions.AccessStatistics.Name) {
                        context.Adjusted = true;
                        context.Permission = Permissions.AccessSpecificQuestionnaireStatistics;
                    }

                    // Check for the permission to export to a specific questionnaire for current user
                    if (context.Permission.Name == Permissions.ExportStatistics.Name) {
                        context.Adjusted = true;
                        context.Permission = Permissions.ExportSpecificQuestionnaireStatistics;
                    }
                }
            } 
        }

        public void Checking(CheckAccessContext context) { }

        public void Complete(CheckAccessContext context) {
            //If we granted permissions for the content, we still need to check for the
            //permission specific for questionnaires. This is because if a Role has the "ViewContent"
            //permission it will be able to view everything even though it does not have the
            //permission that is specific to questionnaires
            if (context.Granted &&
                context.Content.Is<QuestionnairePart>()) {

                context.Granted = _authorizer.Authorize(Permissions.SubmitQuestionnaire);
            }

            if (context.Granted &&
                context.Content.Is<QuestionnaireSpecificAccessPart>()) {

                if (context.Permission.Name == Permissions.AccessSpecificQuestionnaireStatistics.Name ||
                    context.Permission.Name == Permissions.ExportSpecificQuestionnaireStatistics.Name) {

                    var accessPart = context.Content.As<QuestionnaireSpecificAccessPart>();
                    if (accessPart != null) {
                        context.Granted = accessPart.UserIds
                            .Contains(_orchardServices.WorkContext.CurrentUser.Id);
                    }
                }
            }
        }
    }
}