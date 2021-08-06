using AutoMapper;
using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.Services;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Users.Models;
using System.Linq;

namespace Laser.Orchard.Facebook.Drivers {

    public class FacebookAccountDriver : ContentPartDriver<FacebookAccountPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IFacebookService _facebookService;
        private readonly IContentManager _contentManager;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Facebook"; }
        }

        public FacebookAccountDriver(IOrchardServices orchardServices, IFacebookService facebookService,
                                     IContentManager contentManager) {
            _orchardServices = orchardServices;
            _facebookService = facebookService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
        }

        protected override DriverResult Editor(FacebookAccountPart part, dynamic shapeHelper) {
            FacebookAccountVM vm = new FacebookAccountVM();

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<FacebookAccountPart, FacebookAccountVM>();
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            _mapper.Map(part, vm);

            return ContentShape("Parts_FacebookAccount",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/FacebookAccount",
                                    Model: vm,
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(FacebookAccountPart part, IUpdateModel updater, dynamic shapeHelper) {
            FacebookAccountVM vm = new FacebookAccountVM();
            updater.TryUpdateModel(vm, Prefix, null, null);

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<FacebookAccountVM, FacebookAccountPart>();
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            _mapper.Map(vm, part);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(FacebookAccountPart part, ImportContentContext context) {
            var IdPage = context.Attribute(part.PartDefinition.Name, "IdPage");
            if (IdPage != null) {
                part.IdPage = IdPage;
            }
            context.ImportAttribute(part.PartDefinition.Name, "UserIdentity", x => {
                var user = context.ContentManager.Query("User").Where<UserPartRecord>(y => y.UserName == x).List().FirstOrDefault();
                if (user != null) {
                    part.IdUser = user.Id;
                }
            });
            var SocialName = context.Attribute(part.PartDefinition.Name, "SocialName");
            if (SocialName != null) {
                part.SocialName = SocialName;
            }
            var AccountType = context.Attribute(part.PartDefinition.Name, "AccountType");
            if (AccountType != null) {
                part.AccountType = AccountType;
            }
            var UserToken = context.Attribute(part.PartDefinition.Name, "UserToken");
            if (UserToken != null) {
                part.UserToken = UserToken;
            }
            var PageToken = context.Attribute(part.PartDefinition.Name, "PageToken");
            if (PageToken != null) {
                part.PageToken = PageToken;
            }
            var Shared = context.Attribute(part.PartDefinition.Name, "Shared");
            if (Shared != null) {
                part.Shared = bool.Parse(Shared);
            }
            var PageName = context.Attribute(part.PartDefinition.Name, "PageName");
            if (PageName != null) {
                part.PageName = PageName;
            }
            var Valid = context.Attribute(part.PartDefinition.Name, "Valid");
            if (Valid != null) {
                part.Valid = bool.Parse(Valid);
            }
            var DisplayAs = context.Attribute(part.PartDefinition.Name, "DisplayAs");
            if (DisplayAs != null) {
                part.DisplayAs = DisplayAs;
            }
            var UserName = context.Attribute(part.PartDefinition.Name, "UserName");
            if (UserName != null) {
                part.UserName = UserName;
            }
            var UserIdFacebook = context.Attribute(part.PartDefinition.Name, "UserIdFacebook");
            if (UserIdFacebook != null) {
                part.UserIdFacebook = UserIdFacebook;
            }
            if (part.Has<CommonPart>()) {
                part.As<CommonPart>().Owner = _orchardServices.WorkContext.CurrentUser;
            }
        }
        protected override void Exporting(FacebookAccountPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("IdPage", part.IdPage);
            if (part.IdUser > 0) {
                //cerco il corrispondente valore dello username dell'utente
                var contItemUser = _contentManager.Get<UserPart>(part.IdUser);
                if (contItemUser != null) {
                    root.SetAttributeValue("UserIdentity", contItemUser.UserName);
                }
            }
            root.SetAttributeValue("SocialName", part.SocialName);
            root.SetAttributeValue("AccountType", part.AccountType);
            root.SetAttributeValue("UserToken", part.UserToken);
            root.SetAttributeValue("PageToken", part.PageToken);
            root.SetAttributeValue("Shared", part.Shared);
            root.SetAttributeValue("PageName", part.PageName);
            root.SetAttributeValue("Valid", part.Valid);
            root.SetAttributeValue("DisplayAs", part.DisplayAs);
            root.SetAttributeValue("UserIdFacebook", part.UserIdFacebook);
            root.SetAttributeValue("UserName", part.UserName);
        }
    }
}