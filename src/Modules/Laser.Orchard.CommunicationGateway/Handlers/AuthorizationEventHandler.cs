using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement;
using Orchard.Security;
using Contents = Orchard.Core.Contents; //need this because Permissions class for Contacts is called like the default one

namespace Laser.Orchard.CommunicationGateway.Handlers {
    public class AuthorizationEventHandler : IAuthorizationServiceEventHandler {

        private readonly IAuthorizer _authorizer;
        public AuthorizationEventHandler(IAuthorizer authorizer) {

            _authorizer = authorizer;
        }
        public void Adjust(CheckAccessContext context) {
            if (!context.Granted && 
                context.Content.Is<CommunicationContactPart>()) {
                //If we are trying to view a contact, check for the correct permission
                if (context.Permission.Name == Contents.Permissions.ViewContent.Name) {
                    context.Adjusted = true;
                    context.Permission = Permissions.ShowContacts; //will check this permission next
                }

            }
        }

        public void Checking(CheckAccessContext context) { }

        public void Complete(CheckAccessContext context) {
            //if we granted permission for the contact, we still need to check for the 
            //ShowContacts permission. This is because if the role for the user has "ViewContent" permission
            //it will be able to view everything.
            //With ShowContacts we want to restrict that.
            if (context.Granted &&
                context.Content.Is<CommunicationContactPart>()) {
                context.Granted = _authorizer.Authorize(Permissions.ShowContacts);
            }
        }
    }
}