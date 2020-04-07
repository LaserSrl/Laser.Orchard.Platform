
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Laser.Orchard.UsersExtensions.Models;
using System.Linq;
using System;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.Policy.ViewModels;
using Orchard.Security;
using Orchard.Users.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.UsersExtensions.Services;
using Orchard;

namespace Laser.Orchard.UsersExtensions.Handlers {

    public class UserRegistrationPolicyPartHandler : ContentHandler {
        private readonly IUtilsServices _utilsServices;
        private readonly IPolicyServices _policyServices;
        private readonly IUsersExtensionsServices _usersExtensionsServices;
        private readonly IOrchardServices _orchardServices;
        private readonly IControllerContextAccessor _controllerAccessor;

        public UserRegistrationPolicyPartHandler(IUtilsServices utilsServices, IPolicyServices policyServices, IUsersExtensionsServices usersExtensionsServices, IOrchardServices orchardServices, IControllerContextAccessor controllerAccessor) {
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
            _policyServices = policyServices;
            _usersExtensionsServices = usersExtensionsServices;
            _orchardServices = orchardServices;
            _controllerAccessor = controllerAccessor;

        }

        protected override void Published(PublishContentContext context) { // L'update delle risposte può essere fatto solo nel published, altrimenti si alza un'eccezione "save the transient instance before flushing..."
            base.Published(context);
            var userRegistrationPolicy = context.ContentItem.As<UserRegistrationPolicyPart>();
            if (userRegistrationPolicy == null)
                return;

            if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Policy")) {
                var policies = _usersExtensionsServices.BuildEditorForRegistrationPolicies();

                if (_controllerAccessor.Context != null) {
                    var answers = _controllerAccessor.Context.Controller.TempData["VolatileAnswers"] != null ? _controllerAccessor.Context.Controller.TempData["VolatileAnswers"].ToString() : ""; //userRegistrationPolicy.VolatileAnswers;

                    if (!String.IsNullOrWhiteSpace(answers)) {
                        var updateModel = policies.Select(x => new PolicyForUserViewModel {
                            PolicyTextId = x.PolicyId,
                            Accepted = answers.Split(',').Contains(x.PolicyId.ToString()),
                            AnswerDate = DateTime.MinValue, // verrà automaticamente valorizzata in fase di salvataggio
                            OldAccepted = false
                            // non valorizza UserId in caso di nuove policy perché viene valorizzato dal metodo che le salva
                        }).ToList();
                        if (context.ContentItem.As<UserPart>() != null) {
                            _policyServices.PolicyForUserMassiveUpdate(updateModel, (IUser)context.ContentItem.As<UserPart>());
                        }
                        else {
                            _policyServices.PolicyForItemMassiveUpdate(updateModel, context.ContentItem);

                        }
                        _controllerAccessor.Context.Controller.TempData["VolatileAnswers"] = null;
                    }
                }
            }
        }

        public Localizer T { get; set; }
    }
}