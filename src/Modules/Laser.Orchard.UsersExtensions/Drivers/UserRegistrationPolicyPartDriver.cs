using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Web;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.UsersExtensions.Models;
using Laser.Orchard.UsersExtensions.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Models;

namespace Laser.Orchard.UsersExtensions.Drivers {
    public class UserRegistrationPolicyPartDriver : ContentPartDriver<UserRegistrationPolicyPart> {
        private const string CONTROLLER_ACTION = "account/register";
        private readonly IUtilsServices _utilsServices;
        private readonly IUsersExtensionsServices _usersExtensionsServices;
        private readonly IPolicyServices _policyServices;
        private readonly IControllerContextAccessor _controllerAccessor;
        private readonly IOrchardServices _orchardServices;

        public UserRegistrationPolicyPartDriver(IUtilsServices utilsServices, IUsersExtensionsServices usersExtensionsServices, IPolicyServices policyServices, IControllerContextAccessor controllerAccessor, IOrchardServices orchardServices) {
            T = NullLocalizer.Instance;
            Log = NullLogger.Instance;
            _utilsServices = utilsServices;
            _usersExtensionsServices = usersExtensionsServices;
            _policyServices = policyServices;
            _controllerAccessor = controllerAccessor;
            _orchardServices = orchardServices;
        }
        public Localizer T { get; set; }

        private ILogger Log { get; set; }

        private string currentControllerAction {
            get { //MVC 4
                return (_controllerAccessor.Context.RouteData.Values["controller"] + "/" + _controllerAccessor.Context.RouteData.Values["action"]).ToLowerInvariant();
            }
        }

        //GET
        protected override DriverResult Editor(UserRegistrationPolicyPart part, dynamic shapeHelper) {
            if (currentControllerAction != CONTROLLER_ACTION) return null;
            var settings = _orchardServices.WorkContext.CurrentSite.As<UserRegistrationSettingsPart>();
            if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Policy") && settings.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.Yes) {
                var shapeName = "Parts_UserRegistrationPolicy_Edit";
                var templateName = "Parts/UserRegistrationPolicy_Edit";
                var policies = _usersExtensionsServices.BuildEditorForRegistrationPolicies();

                return ContentShape(shapeName,
                                    () => shapeHelper.EditorTemplate(TemplateName: templateName,
                                        Model: policies,
                                        Prefix: Prefix));
            } else {
                return null;
            }
        }

        //POST
        protected override DriverResult Editor(UserRegistrationPolicyPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (currentControllerAction != CONTROLLER_ACTION) return null;
            var settings = _orchardServices.WorkContext.CurrentSite.As<UserRegistrationSettingsPart>();
            if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Policy") && settings.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.Yes) {
                var policies = _usersExtensionsServices.BuildEditorForRegistrationPolicies();
                if (updater.TryUpdateModel(policies, Prefix, null, null)) {
                    if (policies.Count(x => (
                            (x.PolicyAnswer == false) && x.UserHaveToAccept)) > 0) {
                        updater.AddModelError("NotAcceptedPolicies", T("User has to accept policies!"));
                    }
                    _controllerAccessor.Context.Controller.TempData["VolatileAnswers"] = String.Join(",", policies.Where(x => x.PolicyAnswer).Select(x => x.PolicyId.ToString()));
                }
            }
            return Editor(part, shapeHelper);
        }

    }
}