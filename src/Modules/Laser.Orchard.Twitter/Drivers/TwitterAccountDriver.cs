using AutoMapper;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.Services;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Users.Models;
using System.Linq;

namespace Laser.Orchard.Twitter.Drivers {

    public class TwitterAccountDriver : ContentPartDriver<TwitterAccountPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly ITwitterService _TwitterService;
        IContentManager _contentManager;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Twitter"; }
        }

        public TwitterAccountDriver(IOrchardServices orchardServices, ITwitterService TwitterService,
                                    IContentManager contentManager) {
            _orchardServices = orchardServices;
            _TwitterService = TwitterService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
        }

        protected override DriverResult Editor(TwitterAccountPart part, dynamic shapeHelper) {
            TwitterAccountVM vm = new TwitterAccountVM();

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<TwitterAccountPart, TwitterAccountVM>();
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            _mapper.Map<TwitterAccountPart, TwitterAccountVM>(part, vm);

            return ContentShape("Parts_TwitterAccount",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/TwitterAccount",
                                    Model: vm,
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(TwitterAccountPart part, IUpdateModel updater, dynamic shapeHelper) {
            TwitterAccountVM vm = new TwitterAccountVM();
            updater.TryUpdateModel(vm, Prefix, null, null);

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<TwitterAccountVM, TwitterAccountPart>();
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            _mapper.Map<TwitterAccountVM, TwitterAccountPart>(vm, part);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(TwitterAccountPart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "UserIdentity", x => {
                var user = context.ContentManager.Query("User").Where<UserPartRecord>(y => y.UserName == x).List().FirstOrDefault();
                if (user != null) {
                    part.IdUser = user.Id;
                }
            });

            var importedSocialName = context.Attribute(part.PartDefinition.Name, "SocialName");
            if (importedSocialName != null) {
                part.SocialName = importedSocialName;
            }

            var importedAccountType = context.Attribute(part.PartDefinition.Name, "AccountType");
            if (importedAccountType != null) {
                part.AccountType = importedAccountType;
            }

            var importedUserTokenSecret = context.Attribute(part.PartDefinition.Name, "UserTokenSecret");
            if (importedUserTokenSecret != null) {
                part.UserTokenSecret = importedUserTokenSecret;
            }

            var importedShared = context.Attribute(part.PartDefinition.Name, "Shared");
            if (importedShared != null) {
                part.Shared = bool.Parse(importedShared);
            }

            var importedValid = context.Attribute(part.PartDefinition.Name, "Valid");
            if (importedValid != null) {
                part.Valid = bool.Parse(importedValid);
            }

            var importedDisplayAs = context.Attribute(part.PartDefinition.Name, "DisplayAs");
            if (importedDisplayAs != null) {
                part.DisplayAs = importedDisplayAs;
            }
        }
        protected override void Exporting(TwitterAccountPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            if (part.IdUser > 0) {
                //cerco il corrispondente valore dello username dell'utente
                var contItemUser = _contentManager.Get<UserPart>(part.IdUser);
                if (contItemUser != null) {
                    root.SetAttributeValue("UserIdentity", contItemUser.UserName);
                }
            }
            root.SetAttributeValue("SocialName", part.SocialName);
            root.SetAttributeValue("AccountType", part.AccountType);
            root.SetAttributeValue("UserTokenSecret", part.UserTokenSecret);
            root.SetAttributeValue("Shared", part.Shared);
            root.SetAttributeValue("Valid", part.Valid);
            root.SetAttributeValue("DisplayAs", part.DisplayAs);
        }
    }
}