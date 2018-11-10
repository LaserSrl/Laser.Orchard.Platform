using Laser.Orchard.UserProfiler.Service;
using Orchard;
using System.Web.Mvc;

namespace Laser.Orchard.UserProfiler.Controllers {

    public class ProfileController : Controller {
        private readonly IUserProfilingService _userProfilingService;
        private readonly IOrchardServices _orchardServices;

        public ProfileController(
            IOrchardServices orchardServices,
            IUserProfilingService userProfilingService) {
            _userProfilingService = userProfilingService;
            _orchardServices = orchardServices;
        }

        [Authorize]
        [HttpPost]
        public void PostId(int id) {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null && id > 0) {
                var tmp = _userProfilingService.UpdateUserProfile(currentUser.Id, id);
            }
        }
    }
}