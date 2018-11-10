using System.Collections.Generic;
using Laser.Orchard.CulturePicker.Services;
using Laser.Orchard.MultiStepAuthentication.Models;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Logging;
using Orchard.Messaging.Services;
using Orchard.Security;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.NonceTemplateEmail")]
    public class NonceLinkDeliveryByTemplateMailService : IOTPDeliveryService {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly INonceLinkProvider _nonceLinkProvider;
        private readonly IMessageService _messageService;
        private readonly IContentManager _contentManager;
        private readonly ITemplateService _templateService;
        private readonly ILocalizableContentService _localizableContentService;
        public ILogger Logger { get; set; }
        public NonceLinkDeliveryByTemplateMailService(
            IWorkContextAccessor workContextAccessor,
            INonceLinkProvider nonceLinkProvider,
            IMessageService messageService,
            IContentManager contentManager,
            ITemplateService templateService,
            ILocalizableContentService localizableContentService) {
            _localizableContentService = localizableContentService;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _nonceLinkProvider = nonceLinkProvider;
            _messageService = messageService;
            _templateService = templateService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T;

        public DeliveryChannelType ChannelType {
            get { return DeliveryChannelType.Email; }
            set { }
        }

        public int Priority {
            get { return (int)PriorityDelivery.TempleteEmail; ; }
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

            // get link
            var link = _nonceLinkProvider.FormatURI(otp.Password, flow);
            var userlang = _workContextAccessor.GetContext().CurrentSite.SiteCulture;
            if (user.ContentItem.As<FavoriteCulturePart>() != null)
                userlang = user.ContentItem.As<FavoriteCulturePart>().Culture;
            var templatePart = _workContextAccessor.GetContext().CurrentSite.As<NonceTemplateSettingsPart>().SelectedTemplate;
            int templateidToUse = 0;
            if (templatePart != null)
                templateidToUse = templatePart.Id;
            if (templatePart.ContentItem.As<LocalizationPart>() != null) {
                int translatedId = 0;
                if (_localizableContentService.TryGetLocalizedId(templateidToUse, userlang, out translatedId))
                    if (translatedId > 0)
                        templateidToUse = translatedId;
            }
            if (templateidToUse == 0) {
                Logger.Error("NonceTemplatePart must be added to CustomTemplate used for nonce");
                return false;
            }
            else {
                dynamic contentModel = new {
                    ContentItem = user,
                    Link = link
                };
                List<string> sendTo = new List<string>(new string[] { user.Email });
                _templateService.SendTemplatedEmail(contentModel, templateidToUse, sendTo, null);
                return true;
            }

        }
    }
}