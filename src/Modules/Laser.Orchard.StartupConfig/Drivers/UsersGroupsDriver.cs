using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard;
using Orchard.Localization;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Environment.Extensions;
using Laser.Orchard.StartupConfig.Settings;


namespace Laser.Orchard.StartupConfig.Drivers {
    [OrchardFeature("Laser.Orchard.StartupConfig.PermissionExtension")]
    public class UsersGroupsDriver : ContentPartDriver<UsersGroupsPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IUsersGroupsSettingsService _usersGroupsSettingsService;
        public Localizer T { get; set; }


        protected override string Prefix {
            get { return "Laser.StartupConfig.UsersGroups"; }
        }

        public UsersGroupsDriver(IOrchardServices orchardServices, IUsersGroupsSettingsService usersGroupsSettingsService) {
            _orchardServices = orchardServices;
            _usersGroupsSettingsService = usersGroupsSettingsService;

        }

        protected override DriverResult Editor(UsersGroupsPart part, dynamic shapeHelper) {
//var model = new UserInformationVM(part);
            var model = new UsersGroupsVM();
            var settings = part.Settings.GetModel<UserGroupSettings>();

            model.Required = settings.Required;
       //     var getpart = _orchardServices.WorkContext.CurrentSite.As<UsersGroupsSettingsPart>();
        //    var tempviewModel = new UsersGroupsSettingsVM(getpart);
            //   model.ListOfGroups=new List<UsersGroups>();
            model.ListOfGroups = new List<ExtendedUsersGroupsRecordVM>();
            List<ExtendedUsersGroupsRecordVM> vm = _usersGroupsSettingsService.ReadSettings().ExtendedUsersGroupsListVM;
           foreach(ExtendedUsersGroupsRecordVM evm in vm){
               ExtendedUsersGroupsRecordVM ex = new ExtendedUsersGroupsRecordVM();
               ex.GroupName = evm.GroupName;
               ex.Id = evm.Id;
               model.ListOfGroups.Add(ex);
           }
          //  model.ListOfGroups = _usersGroupsSettingsService.ReadSettings().ExtendedUsersGroupsListVM; //tempviewModel.ListGroup;
            try { model.GroupNumber = part.UserGroup.Split(',').Select(x=>Convert.ToInt32(x)).ToList(); }
            catch {model.GroupNumber=new List<int>(); }
            return ContentShape("Parts_UsersGroups", () => shapeHelper.EditorTemplate(TemplateName: "Parts/UsersGroups", Model: model, Prefix: Prefix));


        }

        protected override DriverResult Editor(UsersGroupsPart part, IUpdateModel updater, dynamic shapeHelper) {
           // var model = new UserInformationVM(part);
            var model = new UsersGroupsVM();
            model.ListOfGroups = new List<ExtendedUsersGroupsRecordVM>();
            List<ExtendedUsersGroupsRecordVM> vm = _usersGroupsSettingsService.ReadSettings().ExtendedUsersGroupsListVM;
            foreach (ExtendedUsersGroupsRecordVM evm in vm) {
                ExtendedUsersGroupsRecordVM ex = new ExtendedUsersGroupsRecordVM();
                ex.GroupName = evm.GroupName;
                ex.Id = evm.Id;
                model.ListOfGroups.Add(ex);
            }
            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                //if (model.GroupNumber.Count()>0)
                if (model.GroupNumber == null)
                    part.UserGroup = "";
                else
                    part.UserGroup = string.Join(",", model.GroupNumber);
            }
            else {
               // throw new OrchardException(T("Select a Group please"));
             //  updater.AddModelError("Error Saving Content Item", T("Error Saving Content Item"));
            }
            var settings = part.Settings.GetModel<UserGroupSettings>();

            //if (settings.Required && model.GroupNumber == null) {
            //    updater.AddModelError("User Groups", T("Please select a group"));
            //}

            
            model.ListOfGroups = new List<ExtendedUsersGroupsRecordVM>();
            List<ExtendedUsersGroupsRecordVM> li = _usersGroupsSettingsService.ReadSettings().ExtendedUsersGroupsListVM;
            foreach (var el in li) {
                ExtendedUsersGroupsRecordVM nuovo = new ExtendedUsersGroupsRecordVM();
                nuovo.Id = el.Id;
                nuovo.GroupName = el.GroupName;
                model.ListOfGroups.Add(nuovo);
            }
         //   model.ListOfGroups = _usersGroupsSettingsService.ReadSettings().ExtendedUsersGroupsListVM; 
            //  var viewModel = new PublishRequestVM();
            return ContentShape("Parts_UsersGroups", () => shapeHelper.EditorTemplate(TemplateName: "Parts/UsersGroups", Model: model, Prefix: Prefix));
        }

        protected override void Importing(UsersGroupsPart part, ImportContentContext context) {
            var importedUserGroup = context.Attribute(part.PartDefinition.Name, "UserGroup");
            if (importedUserGroup != null) {
                part.UserGroup = importedUserGroup;
            }
        }

        protected override void Exporting(UsersGroupsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserGroup", part.UserGroup);
        }
    }


    

}