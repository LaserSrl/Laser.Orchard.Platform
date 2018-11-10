using System.Collections.Generic;
using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard;
using Orchard.Email.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Security;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class DefaultNonceLinkDeliveryByMailService : IOTPDeliveryService {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly INonceLinkProvider _nonceLinkProvider;
        private readonly IMessageService _messageService;

        public DefaultNonceLinkDeliveryByMailService(
            IWorkContextAccessor workContextAccessor,
            INonceLinkProvider nonceLinkProvider,
            IMessageService messageService) {

            _workContextAccessor = workContextAccessor;
            _nonceLinkProvider = nonceLinkProvider;
            _messageService = messageService;
            T = NullLocalizer.Instance;
        }

        public Localizer T;

        public DeliveryChannelType ChannelType {
            get { return DeliveryChannelType.Email; }
            set { }
        }

        public int Priority {
            get { return (int)PriorityDelivery.Defaultmail; }
            set { }
        }

        public bool TrySendOTP(OTPRecord otp, IUser user) {
            return TrySendOTP(otp, user, null);
        }

        public bool TrySendOTP(OTPRecord otp, IUser user, FlowType? flow) {
            if (otp == null // parameter validation
                || user == null
                || otp.UserRecord.UserName != user.UserName) {
                return false;
            }
            var currentSite = _workContextAccessor.GetContext().CurrentSite;
            var data = new Dictionary<string, object>();
            //// get link
            var link = _nonceLinkProvider.FormatURI(otp.Password, flow);
            data.Add("Subject", T("{0} - Login", currentSite.SiteName).Text);
            data.Add("Body", T("<html><body>To login on \"{0}\", please open the following link: <a href=\"{1}\">Login</a></body></html>", currentSite.SiteName, link).Text);
            data.Add("Recipients", user.Email);
            _messageService.Send(SmtpMessageChannel.MessageType, data);
            return true;
        }
    }
}