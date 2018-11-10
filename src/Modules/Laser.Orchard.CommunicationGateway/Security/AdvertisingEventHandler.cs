using System.Web.UI.WebControls;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;
using Orchard.Security.Permissions;
using OCore = Orchard.Core;

namespace Laser.Orchard.CommunicationGateway.Security {
    public class AdvertisingEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (!context.Granted &&
                context.Content.Is<ICommonPart>()) {
                    if ((context.Permission.Name == OCore.Contents.Permissions.PublishContent.Name && context.Content.ContentItem.ContentType == "CommunicationAdvertising") || (context.Permission.Name == OCore.Contents.Permissions.PublishOwnContent.Name && context.Content.ContentItem.ContentType == "CommunicationAdvertising")) {
                    context.Adjusted = true;
                    context.Permission = Permissions.ManageCommunicationAdv;
                }
                else if (OwnerVariationExists(context.Permission) &&
                    HasOwnership(context.User, context.Content)) {
                    context.Adjusted = true;
                    context.Permission = GetOwnerVariation(context.Permission);
                }
            }
        }

        private static bool HasOwnership(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            if (HasOwnershipOnContainer(user, content)) {
                return true;
            }

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;

            return user.Id == common.Owner.Id;
        }

        private static bool HasOwnershipOnContainer(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Container == null)
                return false;

            common = common.Container.As<ICommonPart>();
            if (common == null || common.Container == null)
                return false;

            return user.Id == common.Owner.Id;
        }

        private static bool OwnerVariationExists(Permission permission) {
            return GetOwnerVariation(permission) != null;
        }

        private static Permission GetOwnerVariation(Permission permission) {
            if (permission.Name == Permissions.PublishCommunicationAdv.Name)
                return Permissions.PublishOwnCommunicationAdv;
            if (permission.Name == Permissions.ManageCampaigns.Name)
                return Permissions.ManageOwnCampaigns;
            if (permission.Name == Permissions.ManageCommunicationAdv.Name)
                return Permissions.ManageOwnCommunicationAdv;
            if (permission.Name == Permissions.ManageContact.Name)
                return Permissions.ManageOwnContact;
            return null;
        }
    }
}