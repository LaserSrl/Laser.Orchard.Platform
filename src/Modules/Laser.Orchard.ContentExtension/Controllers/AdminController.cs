using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
//using Laser.Orchard.CulturePicker.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard;
using Laser.Orchard.ContentExtension.Models;
using Laser.Orchard.ContentExtension.Services;
using System.Collections.Generic;

using Orchard.ContentTypes.Services;
using Orchard.Roles.Services;
using Orchard.Security.Permissions;
using System.Collections;



namespace Laser.Orchard.ContentExtension.Controllers {
    public class AdminController : Controller {
        private readonly IContentTypePermissionSettingsService _contentTypePermissionSettingsService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        // private readonly IPermissionProvider _permissionProvider;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly IRoleService _roleService;

        public Localizer T { get; set; }
        // GET: /Admin/
        public AdminController(IContentTypePermissionSettingsService contentTypePermissionSettingsService,
            IAuthenticationService authenticationService,
            IMembershipService membershipService, IOrchardServices orcharcServices,
            IContentDefinitionService contentDefinitionService,
            IRoleService roleService
            //       IPermissionProvider permissionProvider
            ) {
            _contentTypePermissionSettingsService = contentTypePermissionSettingsService;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _orchardServices = orcharcServices;
            T = NullLocalizer.Instance;
            _contentDefinitionService = contentDefinitionService;
            _roleService = roleService;
            //      _permissionProvider = permissionProvider;
        }

        [HttpGet]
        public ActionResult Settings() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit ContentTypePermission settings!")))
                return new HttpUnauthorizedResult();
            var model = _contentTypePermissionSettingsService.ReadSettings();
            var listconttype = _contentDefinitionService.GetTypes().Select(x => new SelectListItem() { Text = x.DisplayName, Value = x.Name }).ToList();
            listconttype.Insert(0, new SelectListItem { Text = " ", Value = " " });

            ViewData["ListContentTypes"] = new SelectList(listconttype, "Value", "Text");
            //_orchardServices..ContentManager.GetContentTypeDefinitions()
            var tmplistpermissions = _roleService.GetInstalledPermissions();
            List<SelectListItem> listpermissions = new List<SelectListItem>();
            foreach (IEnumerable<Permission> sad in tmplistpermissions.Values) {
                foreach (Permission perm in sad) {
                    listpermissions.Add(new SelectListItem { Text = perm.Name, Value = perm.Name });
                }
            }
            listpermissions.Insert(0, new SelectListItem { Text = "", Value = "" });
            ViewData["ListPermissions"] = new SelectList(listpermissions.OrderBy(x => x.Text), "Value", "Text");
            // var listpermissions = _permissionProvider.GetPermissions();
            //  listpermissions.Select(x => new SelectListItem() { Text = x.Name, Value = x.Name });

            //    IDictionary<string, IEnumerable<Orchard.Security.Permissions.Permission>>



            //  .Select(x => new SelectListItem() { Text = x.Key, Value = x.Key });

            //_orchardServices.ContentManager.GetContentTypeDefinitions().Select(x => x.Name).ToList();

            //  var listpermissions = _contentDefinitionService.GetTypes().Select(x => new SelectListItem() { Text = x.DisplayName, Value = x.Name });
            //_orchardServices..ContentManager.GetContentTypeDefinitions()

            //      ViewData["ListContentTypes"] = new SelectList(listconttype, "Value", "Text");

            ContentTypePermissionRecord cpr = new ContentTypePermissionRecord();
            cpr.Id = 0;
            model.ListContPermission.Add(cpr);
            return View(model);
        }
        [HttpPost]
        public ActionResult Settings(SettingsModel model) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit ContentTypePermission settings!")))
                return new HttpUnauthorizedResult();
            if (!ModelState.IsValid) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", T("check your input!")));
                return View(model);
            }
            try {
                _contentTypePermissionSettingsService.WriteSettings(model);
                _orchardServices.Notifier.Information(T("ContentType Permission settings updated."));
                // I read again my model in order to its ids
                model = _contentTypePermissionSettingsService.ReadSettings();
            } catch (Exception exception) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", exception.Message));
            }
            return RedirectToAction("Settings");
        }
    }
}
