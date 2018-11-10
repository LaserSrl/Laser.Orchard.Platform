using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.UserProfiler.Service;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Orchard.OutputCache;
using System.Text;

namespace Laser.Orchard.CommunicationGateway.Drivers {

    public class CommunicationContactDriver : ContentPartDriver<CommunicationContactPart>, ICachingEventHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IContentManager _contentManager;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        private readonly IUtilsServices _utilsService;

        protected override string Prefix {
            get { return "Laser.Orchard.CommunicationGateway"; }
        }

        public CommunicationContactDriver(IOrchardServices orchardServices, IUtilsServices utilsService, IControllerContextAccessor controllerContextAccessor, IContentManager contentManager) {

            _orchardServices = orchardServices;
            _controllerContextAccessor = controllerContextAccessor;
            _utilsService = utilsService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;

            _isAuthorized = new Lazy<bool>(() =>
                _orchardServices.Authorizer.Authorize(Permissions.ShowContacts)
            );
        }

        private Lazy<bool> _isAuthorized;
        private bool IsAuthorized {
            get {
                return _isAuthorized.Value;
            }
        }

        protected override DriverResult Display(CommunicationContactPart part, string displayType, dynamic shapeHelper) {
            // check sulle permission (esclude il modulo Generator)
            if (_controllerContextAccessor.Context.Controller.GetType().Namespace != "Laser.Orchard.Generator.Controllers") {
                if (!IsAuthorized) {
                    throw new System.Security.SecurityException("You do not have permission to access this content.");
                }
            }
            if (_utilsService.FeatureIsEnabled("Laser.Orchard.UserProfiler")) {
                IUserProfilingService _userProfilingService;
                if (_orchardServices.WorkContext.TryResolve<IUserProfilingService>(out _userProfilingService)) {
                    var profiling = _userProfilingService.GetList(part.UserIdentifier);
                    ((dynamic)(part.ContentItem)).ContactProfilingPart.Profiling = profiling;
                }
            }
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Detail") {
                    string logs = T("No log.").Text;
                    if (string.IsNullOrWhiteSpace(part.Logs) == false) {
                        logs = part.Logs;
                    }
                    var profile = part.ContentItem.Parts.FirstOrDefault(x => x.PartDefinition.Name == "ProfilePart");
                    return Combined(ContentShape("Parts_CommunicationContact",
                        () => shapeHelper.Parts_CommunicationContact(Logs: logs)),
                        ContentShape("Parts_ProfilePart",
                        () => shapeHelper.Parts_ProfilePart(ContentPart: profile))
                            );
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        }

        protected override DriverResult Editor(CommunicationContactPart part, dynamic shapeHelper) {
            CommunicationContactPartVM model = new CommunicationContactPartVM();
            if (string.IsNullOrWhiteSpace(part.Logs)) {
                model.Logs = T("No log.").Text;
            }
            else {
                model.Logs = part.Logs;
            }
            return ContentShape("Parts_CommunicationContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/CommunicationContact_Edit", Model: model, Prefix: Prefix));
        }

        protected override void Importing(CommunicationContactPart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "UserIdentifier", x => {
                var user = context.ContentManager.Query("User").Where<UserPartRecord>(y => y.UserName == x).List().FirstOrDefault();
                if (user != null) {
                    //associa id user
                    part.UserIdentifier = user.Id;
                }
            });
            var importedMaster = context.Attribute(part.PartDefinition.Name, "Master");
            if (importedMaster != null) {
                part.Master = Convert.ToBoolean(importedMaster);
            }
            var importedLogs = context.Attribute(part.PartDefinition.Name, "Logs");
            if (importedLogs != null) {
                part.Logs = importedLogs;
            }
        }

        protected override void Exporting(CommunicationContactPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            if (part.UserIdentifier > 0) {
                //cerca lo username corrispondente
                var contItemUser = _contentManager.Get(part.UserIdentifier);
                if (contItemUser != null) {
                    root.SetAttributeValue("UserIdentifier", contItemUser.As<UserPart>().UserName);
                }
            }
            root.SetAttributeValue("Master", part.Master);
            root.SetAttributeValue("Logs", part.Logs);
        }

        public void KeyGenerated(StringBuilder key) {
            if (IsAuthorized) {
                key.Append("ShowContacts=true;");
            } else {
                key.Append("ShowContacts=false;");
            }
        }
    }
}