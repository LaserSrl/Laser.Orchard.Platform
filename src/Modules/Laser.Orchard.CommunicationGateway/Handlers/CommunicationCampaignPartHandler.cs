using Laser.Orchard.CommunicationGateway.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.PublishLater.Models;
using Orchard.PublishLater.Services;
using Orchard.UI.Notify;
using System;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class CommunicationCampaignPartHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IPublishLaterService _publishLaterService;
        private readonly ITransactionManager _transactions;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public CommunicationCampaignPartHandler(ITransactionManager transactions, INotifier notifier, IOrchardServices orchardServices, IPublishLaterService publishLaterService) {
            _publishLaterService = publishLaterService;
            _orchardServices = orchardServices;
            _transactions = transactions;
            _notifier = notifier;
            T = NullLocalizer.Instance;

            OnUpdated<CommunicationAdvertisingPart>((context, communicationAdvertisingPart) => {
                ContentItem campaign = null;
                if (communicationAdvertisingPart.CampaignId > 0) {
                    campaign = _orchardServices.ContentManager.Get(communicationAdvertisingPart.CampaignId, VersionOptions.Latest);
                }
                ControlDatePublishLater(communicationAdvertisingPart, campaign);
            });

            OnUpdated<CommunicationCampaignPart>((context, communicationCampaignPart) => {
                var relatedadvertisement = _orchardServices.ContentManager.Query<CommunicationAdvertisingPart, CommunicationAdvertisingPartRecord>().Where(x => x.CampaignId == communicationCampaignPart.Id).List();
                ContentItem campaign = communicationCampaignPart.ContentItem;

                foreach (CommunicationAdvertisingPart cp in relatedadvertisement) {
                    ControlDatePublishLater(cp, campaign);
                }
            });
        }

        private void ControlDatePublishLater(CommunicationAdvertisingPart communicationAdvertisingPart, ContentItem campaign) {
            DateTime? datepublish = _publishLaterService.GetScheduledPublishUtc(communicationAdvertisingPart.ContentItem.As<PublishLaterPart>());
            if (communicationAdvertisingPart.CampaignId > 0 && datepublish != null && campaign != null) {
                DateTime datelimitcampaign = (DateTime)(((dynamic)campaign).CommunicationCampaignPart.ToDate.DateTime);
                if (datepublish > datelimitcampaign && datelimitcampaign!=DateTime.MinValue) {
                    _notifier.Add(NotifyType.Error, T("Cannot Update! publish later date is after Campaign date"));
                    _transactions.Cancel();
                }
            }
        }
    }
}