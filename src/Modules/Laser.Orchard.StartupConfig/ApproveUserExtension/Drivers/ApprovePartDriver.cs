using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Users.Models;
using Orchard.Users.ViewModels;

namespace Laser.Orchard.StartupConfig.ApproveUserExtension.Drivers {
    public class ApprovePartDriver : ContentPartDriver<UserPart> {
        private const string TemplateName = "Parts/UserApprove";
        private readonly IOrchardServices _orchardServices;

        public ApprovePartDriver(
            IOrchardServices orchardServices
            ) {
            _orchardServices = orchardServices;
        }

        private UserEditViewModel BuildViewModelFromPart(int id) {
            var user = _orchardServices.ContentManager.Get(id).As<UserPart>();

            if (user == null)
                return new UserEditViewModel();

            return new UserEditViewModel {
                User = user
            };
        }

        protected override DriverResult Editor(UserPart part, dynamic shapeHelper) {
            var model = BuildViewModelFromPart(part.Id);

            return ContentShape("Parts_UserApprove_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }
    }
}