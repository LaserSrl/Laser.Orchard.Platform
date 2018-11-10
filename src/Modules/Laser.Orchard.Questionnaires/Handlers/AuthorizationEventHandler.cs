using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement;
using Orchard.Security;
using Contents = Orchard.Core.Contents; //need this because Permissions class for Questionnaires is called like the default one

namespace Laser.Orchard.Questionnaires.Handlers {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {

        private readonly IAuthorizer _authorizer;
        public AuthorizationEventHandler(IAuthorizer authorizer) {

            _authorizer = authorizer;
        }
        public void Adjust(CheckAccessContext context) {
            if (!context.Granted && 
                context.Content.Is<QuestionnairePart>()) {
                //If we are trying to view a Questionnaire, check for the correct permission
                if (context.Permission.Name == Contents.Permissions.ViewContent.Name) {
                    context.Adjusted = true;
                    context.Permission = Permissions.SubmitQuestionnaire; //will check this permission next
                }
            }
        }

        public void Checking(CheckAccessContext context) { }

        public void Complete(CheckAccessContext context) {
            //If we granted permissions for the content, we still need to check for the
            //permission specific for questionnaires. Thisd is because if a Rolw has the "ViewContent"
            //permission it will be able to view everything even though it does not have the
            //permission that is specific to questionnaires
            if (context.Granted && 
                context.Content.Is<QuestionnairePart>()) {
                context.Granted = _authorizer.Authorize(Permissions.SubmitQuestionnaire);
            }
        }
    }
}