using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.Mobile.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Users.Models;
using Orchard;
using Laser.Orchard.Mobile.Services;

namespace Laser.Orchard.Mobile.Handlers {

    public class MobileContactPartHandler : ContentHandler {
        private readonly IRepository<PushNotificationRecord> _deviceRepository;
        private readonly IRepository<UserDeviceRecord> _userDeviceRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly IPushNotificationService _pushNotificationService;

        public MobileContactPartHandler(IRepository<MobileContactPartRecord> repository, IRepository<PushNotificationRecord> ProviderRepository, IRepository<UserDeviceRecord> userDeviceRepository, IOrchardServices orchardServices, IPushNotificationService pushNotificationService) {
            Filters.Add(StorageFilter.For(repository));
            _deviceRepository = ProviderRepository;
            _userDeviceRepository = userDeviceRepository;
            _orchardServices = orchardServices;
            _pushNotificationService = pushNotificationService;
            Filters.Add(new ActivatingFilter<MobileContactPart>("CommunicationContact"));
            OnLoaded<MobileContactPart>(LazyLoadHandlers);
            OnRemoved<UserPart>((context, part) => { _pushNotificationService.DeleteUserDeviceAssociation(part.Id); });
        }

        protected void LazyLoadHandlers(LoadContentContext context, MobileContactPart part) {
            // Add handlers that will load content for id's just-in-time
            part.MobileEntries.Loader(() => OnLoader(context));
        }

        private IList<PushNotificationRecord> OnLoader(LoadContentContext context) {
            return _deviceRepository
                    .Fetch(x => x.MobileContactPartRecord_Id == context.ContentItem.Id)
                    .Select(x => new PushNotificationRecord {
                        DataInserimento = x.DataInserimento,
                        Device = x.Device,
                        DataModifica = x.DataModifica,
                        Language = x.Language,
                        Id = x.Id,
                        Produzione = x.Produzione,
                        Token = x.Token,
                        MobileContactPartRecord_Id = x.MobileContactPartRecord_Id,
                        UUIdentifier = x.UUIdentifier,
                        Validated = x.Validated
                    })
                    .ToList();
        }
    }
}