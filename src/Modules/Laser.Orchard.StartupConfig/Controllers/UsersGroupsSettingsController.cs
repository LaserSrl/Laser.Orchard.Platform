using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Mvc;


using System.Linq;

using Laser.Orchard.StartupConfig.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Themes;
using Orchard.UI.Admin;
using OrchardCore=Orchard.Core;


namespace Laser.Orchard.StartupConfig.Controllers {

    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsSettingsController : Controller {
          private readonly IUsersGroupsSettingsService _usersGroupsSettingsService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        public UsersGroupsSettingsController(IUsersGroupsSettingsService usersGroupsSettingsService, 
            IAuthenticationService authenticationService,
            IMembershipService membershipService, IOrchardServices orcharcServices) {
                _usersGroupsSettingsService = usersGroupsSettingsService;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _orchardServices = orcharcServices;
            T = NullLocalizer.Instance;
        }


        [HttpGet]
        [Admin]
        public ActionResult Settings() {
            if (!_orchardServices.Authorizer.Authorize(OrchardCore.Settings.Permissions.ManageSettings, T("You don't have permission \"Manage settings\" to define and manage User Groups!")))
                return new HttpUnauthorizedResult();
            var model = _usersGroupsSettingsService.ReadSettings();
            model.ExtendedUsersGroupsListVM.Insert(0, new ExtendedUsersGroupsRecordVM());
            return View(model);
        }
        [HttpPost]
        [Admin]
        public ActionResult Settings(ViewModels.UsersGroupsSettingsVM model) {
            if (!_orchardServices.Authorizer.Authorize(OrchardCore.Settings.Permissions.ManageSettings, T("You don't have permission \"Manage settings\" to define and manage User Groups!")))
                return new HttpUnauthorizedResult();
            if (!ModelState.IsValid) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", T("check your input!")));
                return View(model);
            }
            try {
                _usersGroupsSettingsService.WriteSettings(model);
                _orchardServices.Notifier.Information(T("Settings updated."));
                // I read again my model in order to its ids
                model = _usersGroupsSettingsService.ReadSettings();
            } catch (Exception exception) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", exception.Message));
            }

            return RedirectToActionPermanent("Settings");
        }








        //private readonly IOrchardServices _orchardServices;
        //private readonly ShellSettings _shellSettings;

        //public UsersGroupsSettingsDriver(IOrchardServices orchardServices, ShellSettings shellSettings) {
        //    _orchardServices = orchardServices;
        //    _shellSettings = shellSettings;
        //}
        //protected override string Prefix {
        //    get { return "Laser.UsersGroupsSettings.Settings"; }
        //}


        //protected override DriverResult Editor(UsersGroupsSettingsPart part, dynamic shapeHelper) {
        //    return Editor(part, null, shapeHelper);

        //}
   

        //protected override DriverResult Editor(UsersGroupsSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

        //    return ContentShape("Parts_UsersGroupsSettings_Edit", () => {
        //        var getpart = _orchardServices.WorkContext.CurrentSite.As<UsersGroupsSettingsPart>();
        //        var viewModel = new UsersGroupsSettingsVM(getpart);
      

        //        if (updater != null) {
        //            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
        //                part.GroupSerialized = viewModel.ListGroup;
        //             }
        //        }
        //        else {
        //            List<Groupdata> list = viewModel.ListGroup;
        //            Groupdata gd=new Groupdata();
        //            gd.GroupName="";
        //            gd.Number = 0;
        //            list.Add(gd);
        //            viewModel.ListGroup = list;
        //        }
        //        return shapeHelper.EditorTemplate(TemplateName: "Parts/UsersGroupsSettings_Edit", Model: viewModel, Prefix: Prefix);
        //    })
        //  .OnGroup("UserGroups");
        //}

        //protected override void Importing(UsersGroupsSettingsPart part, ImportContentContext context) {
        //    throw new NotImplementedException();
        //}

        //protected override void Exporting(UsersGroupsSettingsPart part, ExportContentContext context) {
        //    throw new NotImplementedException();
        //}

    }
}