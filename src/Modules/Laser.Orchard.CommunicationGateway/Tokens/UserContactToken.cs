using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Tokens;
using Orchard.Localization;
using Orchard.Security;
using Orchard;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace Laser.Orchard.CommunicationGateway.Tokens {
    public class UserContactToken : ITokenProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        public Localizer T { get; set; }

        public UserContactToken(IOrchardServices orchardServices, ICommunicationService communicationService) {
            _orchardServices = orchardServices;
            _communicationService = communicationService;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeContext context) {
            context.For("User", T("User Contact"), T("Contact bound to the user"))
                .Token("CommunicationContact", T("UserContact"), T("The contact bound to the user"));
        }
        public void Evaluate(EvaluateContext context) {
            context.For<IUser>("User", _orchardServices.WorkContext.CurrentUser)
                .Token("CommunicationContact", user => GetContactName(user))
                .Chain("CommunicationContact", "Content", user => GetContact(user)); // il primo parametro di .Chain deve essere uguale al primo parametro del .Token che lo precede
        }
        private IContent GetContact(IUser user) {
            if(user != null) {
                var contact = _communicationService.GetContactFromUser(user.Id);
                if (contact != null) {
                    return contact.ContentItem;
                }
            }
            return null;
        }
        private string GetContactName(IUser user) {
            var contact = GetContact(user);
            if (contact != null) {
                return contact.ContentItem.As<TitlePart>().Title;
            }
            else {
                return "";
            }
        }
    }
}