using Laser.Orchard.UserProfiler.Models;
using Laser.Orchard.UserProfiler.Service;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Drivers {

    public class UserProfilingSettingPartDriver : ContentPartDriver<UserProfilingSettingPart> {
        private const string TemplateName = "Parts/UserProfilingSetting";
        private readonly IOrchardServices _orchardServices;
        private readonly IUserProfilingService _userProfilingService;
        protected override string Prefix { get { return "UserProfilingSettingPart"; } }

        public UserProfilingSettingPartDriver(
            IOrchardServices orchardServices,
            IUserProfilingService userProfilingService) {
            _orchardServices = orchardServices;
            _userProfilingService = userProfilingService;
        }

        protected override DriverResult Editor(UserProfilingSettingPart part, dynamic shapeHelper) {
            return ContentShape("Parts_UserProfilingSetting_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(UserProfilingSettingPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(part, Prefix, null, null)) {
                var idsusers = _orchardServices.ContentManager.Query().ForType("User").List().Select(x => x.Id).ToList();
                foreach (var id in idsusers) {
                    _userProfilingService.RefreshTotal(id);
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}