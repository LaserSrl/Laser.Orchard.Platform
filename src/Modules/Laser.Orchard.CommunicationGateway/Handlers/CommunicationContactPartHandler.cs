using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.StartupConfig.Handlers;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class CommunicationContactPartHandler : ContentHandler {
        public Localizer T { get; set; }
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<CommunicationEmailRecord> _Emailrepository;
        private readonly IRepository<CommunicationSmsRecord> _Smsrepository;
        private readonly IContactRelatedEventHandler _contactEventHandler;

        public CommunicationContactPartHandler(IRepository<CommunicationSmsRecord> Smsrepository, IRepository<CommunicationEmailRecord> Emailrepository, IRepository<CommunicationContactPartRecord> repository, ICommunicationService communicationService, IContactRelatedEventHandler contactEventHandler) {
            _Smsrepository = Smsrepository;
            _communicationService = communicationService;
            Filters.Add(StorageFilter.For(repository));
            _Emailrepository = Emailrepository;
            T = NullLocalizer.Instance;
            _contactEventHandler = contactEventHandler;

            Filters.Add(new ActivatingFilter<EmailContactPart>("CommunicationContact"));
            OnLoaded<EmailContactPart>(LazyLoadEmailHandlers);

            Filters.Add(new ActivatingFilter<SmsContactPart>("CommunicationContact"));
            OnLoaded<SmsContactPart>(LazyLoadSmsHandlers);

            Filters.Add(new ActivatingFilter<FavoriteCulturePart>("CommunicationContact"));

            #region sync user profile

            // OnCreated<UserPart>((context, part) => UpdateProfile(context.ContentItem));
            OnUpdated<UserPart>((context, part) => UpdateProfile(context.ContentItem));
            OnRemoved<UserPart>((context, part) => { _communicationService.UnboundFromUser(part); });
            OnRemoved<CommunicationContactPart>((context, part) => {
                _communicationService.RemoveMailsAndSms(part.Id);
                _contactEventHandler.ContactRemoved(part.Id);
            });
            OnUpdated<CommunicationContactPart>((context, part) => _communicationService.ContactToUser(part.ContentItem));
            #endregion sync user profile
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var ccPart = context.ContentItem.As<CommunicationContactPart>();

            if (ccPart == null) {
                return;
            }

            // set the correct path to the action to edit this contact
            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Laser.Orchard.CommunicationGateway"},
                {"Controller", "ContactsAdmin"},
                {"Action", "Edit"},
                {"Id", context.ContentItem.Id}
            };
        }

        protected void LazyLoadEmailHandlers(LoadContentContext context, EmailContactPart part) {
            // Add handlers that will load content for id's just-in-time
            part.EmailEntries.Loader(() => OnEmailLoader(context));
        }

        private IList<CommunicationEmailRecord> OnEmailLoader(LoadContentContext context) {
            return _Emailrepository
                    .Fetch(x => x.EmailContactPartRecord_Id == context.ContentItem.Id)
                    .Select(x => new CommunicationEmailRecord {
                        DataInserimento = x.DataInserimento,
                        DataModifica = x.DataModifica,
                        Language = x.Language,
                        Id = x.Id,
                        Produzione = x.Produzione,
                        Email = x.Email,
                        EmailContactPartRecord_Id = x.EmailContactPartRecord_Id,
                        Validated = x.Validated,
                        AccettatoUsoCommerciale = x.AccettatoUsoCommerciale,
                        AutorizzatoTerzeParti = x.AutorizzatoTerzeParti
                    })
                    .ToList();
        }

        protected void LazyLoadSmsHandlers(LoadContentContext context, SmsContactPart part) {
            // Add handlers that will load content for id's just-in-time
            part.SmsEntries.Loader(() => OnSmsLoader(context));
        }

        private IList<CommunicationSmsRecord> OnSmsLoader(LoadContentContext context) {
            return _Smsrepository
                    .Fetch(x => x.SmsContactPartRecord_Id == context.ContentItem.Id)
                    .Select(x => new CommunicationSmsRecord {
                        DataInserimento = x.DataInserimento,
                        DataModifica = x.DataModifica,
                        Language = x.Language,
                        Id = x.Id,
                        Produzione = x.Produzione,
                        Sms = x.Sms,
                        Prefix = x.Prefix,
                        SmsContactPartRecord_Id = x.SmsContactPartRecord_Id,
                        Validated = x.Validated,
                        AccettatoUsoCommerciale = x.AccettatoUsoCommerciale,
                        AutorizzatoTerzeParti = x.AutorizzatoTerzeParti
                    })
                    .ToList();
        }

        private void UpdateProfile(ContentItem item) {
            if (item.ContentType == "User") {
                _communicationService.UserToContact((IUser)item.As<IUser>());
            }
        }
    }
}