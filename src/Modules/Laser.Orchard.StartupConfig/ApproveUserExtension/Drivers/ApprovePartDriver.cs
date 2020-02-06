using Laser.Orchard.StartupConfig.ApproveUserExtension.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Users.Models;
using Orchard.Users.ViewModels;
using System;

namespace Laser.Orchard.StartupConfig.ApproveUserExtension.Drivers {
    public class ApprovePartDriver : ContentPartDriver<UserPart> {
        private const string TemplateName = "Parts/UserApprove";
        private readonly IApproveUserService _approveUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApprovePartDriver(
            IApproveUserService approveUserService,
            IHttpContextAccessor httpContextAccessor
            ) {

            _approveUserService = approveUserService;
            _httpContextAccessor = httpContextAccessor;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(UserPart part, dynamic shapeHelper) {
            var model = new UserEditViewModel { User = part };

            return ContentShape("Parts_UserApprove_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(UserPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new UserEditViewModel { User = part };

            return ContentShape("Parts_UserApprove_Edit",
                              () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));

        }
    }
}