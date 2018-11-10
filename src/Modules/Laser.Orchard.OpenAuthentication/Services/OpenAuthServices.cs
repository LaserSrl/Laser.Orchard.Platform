using System.Collections.Generic;
using Laser.Orchard.OpenAuthentication.Events;
using Laser.Orchard.OpenAuthentication.Extensions;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Models;
using Laser.Orchard.StartupConfig.Handlers;
using System;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Orchard.Users.Events;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IOpenAuthMembershipServices : IDependency {
        bool CanRegister();
        IUser CreateUser(OpenAuthCreateUserParams createUserParams);
        OpenAuthTemporaryUser CreateTemporaryUser(OpenAuthCreateUserParams createUserParams);
        }

    public class OpenAuthMembershipServices : IOpenAuthMembershipServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IUsernameService _usernameService;
        private readonly IUserEventHandler _userEventHandlers;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly IEnumerable<IOpenAuthUserEventHandler> _openAuthUserEventHandlers;
        private readonly IContactRelatedEventHandler _contactEventHandler;

        public OpenAuthMembershipServices(IOrchardServices orchardServices,
            IMembershipService membershipService,
            IUsernameService usernameService,
            IPasswordGeneratorService passwordGeneratorService,
            IEnumerable<IOpenAuthUserEventHandler> openAuthUserEventHandlers,
            IContactRelatedEventHandler contactEventHandler,
            IUserEventHandler userEventHandlers) {
            _orchardServices = orchardServices;
            _userEventHandlers = userEventHandlers;
            _membershipService = membershipService;
            _usernameService = usernameService;
            _passwordGeneratorService = passwordGeneratorService;
            _openAuthUserEventHandlers = openAuthUserEventHandlers;
            _contactEventHandler = contactEventHandler;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public bool CanRegister() {
            var openAuthenticationSettings = _orchardServices.WorkContext.CurrentSite.As<OpenAuthenticationSettingsPart>();
            return openAuthenticationSettings.AutoRegistrationEnabled;
        }

        public IUser CreateUser(OpenAuthCreateUserParams createUserParams) {
            string emailAddress = string.Empty;
            if (createUserParams.UserName.IsEmailAddress()) {
                emailAddress = createUserParams.UserName;
            }
            else {
                foreach (var key in createUserParams.ExtraData.Keys) {
                    switch (key.ToLower()) {
                        case "mail":
                        case "email":
                        case "e-mail":
                        case "email-address":
                            emailAddress = createUserParams.ExtraData[key];
                            break;
                    }
                }
            }

            createUserParams.UserName = _usernameService.Normalize(createUserParams.UserName);
            var creatingContext = new CreatingOpenAuthUserContext(
                createUserParams.UserName, emailAddress, 
                createUserParams.ProviderName, createUserParams.ProviderUserId, createUserParams.ExtraData);

            _openAuthUserEventHandlers.Invoke(o => o.Creating(creatingContext), Logger);

            // check UserName
            if (String.IsNullOrEmpty(createUserParams.UserName)) {
                return null;
            }
            else {
                // The default IMemebershipService from Orchard.Users fires the following user events:
                // Creating, Created, and Approved (see the last parameter of the CreateUserParams)
                var createdUser = _membershipService.CreateUser(new CreateUserParams(
                    _usernameService.Calculate(createUserParams.UserName), // this tries to make a unique username by adding a number to its end
                    _passwordGeneratorService.Generate(), 
                    creatingContext.EmailAddress,
                    @T("Auto Registered User").Text,
                    _passwordGeneratorService.Generate() /* Noone can guess this */,
                    true
                    ));

                // _membershipService.CreateUser may fail and return null
                if (createdUser != null) {
                    var createdContext = new CreatedOpenAuthUserContext(createdUser, 
                        createUserParams.ProviderName, createUserParams.ProviderUserId, createUserParams.ExtraData);
                    _openAuthUserEventHandlers.Invoke(o => o.Created(createdContext), Logger);
                }
                return createdUser;
            }
        }

        public OpenAuthTemporaryUser CreateTemporaryUser(OpenAuthCreateUserParams createUserParams) {
            string emailAddress = string.Empty;
            string userName = _usernameService.Normalize(createUserParams.UserName);
            if (createUserParams.UserName.IsEmailAddress()) {
                emailAddress = createUserParams.UserName;
            }
            else {
                foreach (var key in createUserParams.ExtraData.Keys) {
                    switch (key.ToLower()) {
                        case "mail":
                        case "email":
                        case "e-mail":
                        case "email-address":
                            emailAddress = createUserParams.ExtraData[key];
                            break;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(userName))
                return null;

            return new OpenAuthTemporaryUser {
                Email = emailAddress,
                UserName = _usernameService.Calculate(_usernameService.Normalize(createUserParams.UserName)),
                Provider = createUserParams.ProviderName,
                ProviderUserId = createUserParams.ProviderUserId
            };
        }
    }

}