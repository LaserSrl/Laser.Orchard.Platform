﻿using Laser.Orchard.HiddenFields.Fields;
using Laser.Orchard.HiddenFields.Security;
using Laser.Orchard.HiddenFields.Settings;
using Laser.Orchard.HiddenFields.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Drivers {
    public class HiddenStringFieldDriver : ContentFieldCloningDriver<HiddenStringField> {

        public Localizer T { get; set; }
        private readonly IOrchardServices _orchardServices;
        private readonly IAuthorizer _authorizer;

        public HiddenStringFieldDriver(IOrchardServices orchardServices, IAuthorizer authorizer) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
            _authorizer = authorizer;
        }

        private static string GetPrefix(HiddenStringField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private string GetDifferentiator(HiddenStringField field, ContentPart part) {
            return GetDifferentiator(field);
        }
        private string GetDifferentiator(HiddenStringField field) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, HiddenStringField field, string displayType, dynamic shapeHelper) {
            //never display an hidden field in front end
            return null; // base.Display(part, field, displayType, shapeHelper);
        }

        //GET
        protected override DriverResult Editor(ContentPart part, HiddenStringField field, dynamic shapeHelper) {
            //require at least the "see" permission
            if (AuthorizeSee(part, field)) {
                return ContentShape("Fields_Hidden_String_Edit", GetDifferentiator(field, part),
                () => {
                    var fs = field.PartFieldDefinition.Settings.GetModel<HiddenStringFieldSettings>();
                    //tokenization happens in the handler
                    var vm = new HiddenStringFieldDriverViewModel {
                        Field = field,
                        Value = field.Value,
                        Settings = fs,
                        IsEditAuthorized = AuthorizeEdit(part,field)
                    };
                    return shapeHelper.EditorTemplate(TemplateName: "Fields.Hidden.String.Edit", Model: vm, Prefix: GetPrefix(field, part));
                });
            }

            // it is better to return an empty shape instead of null
            return ContentShape("Fields_Hidden_String_Edit_Unauthorized", GetDifferentiator(field, part),
             () => {
                 return shapeHelper.EditorTemplate(
                     TemplateName: "Fields.Hidden.String.Edit.Unauthorized", 
                     Model: new HiddenStringFieldDriverViewModel(), 
                     Prefix: GetPrefix(field, part));
             });
        }

        //POST
        protected override DriverResult Editor(ContentPart part, HiddenStringField field, IUpdateModel updater, dynamic shapeHelper) {
            //require at least the "see" permission
            if (AuthorizeSee(part, field)) {
                var fs = field.PartFieldDefinition.Settings.GetModel<HiddenStringFieldSettings>();
                //tokenization happens in the handler
                var vm = new HiddenStringFieldDriverViewModel {
                    Field = field,
                    Value = field.Value,
                    Settings = fs,
                    IsEditAuthorized = _orchardServices.Authorizer.Authorize(HiddenFieldsPermissions.MayEditHiddenFields)
                };
                if (updater.TryUpdateModel(vm, GetPrefix(field, part), null, null)) {
                    field.Value = vm.Value;
                }
            }

            return Editor(part, field, shapeHelper);
        }

        private bool AuthorizeEdit(ContentPart part, HiddenStringField field) {
            return _authorizer.Authorize(new HiddenFieldEditPermission(part, field), part);
        }

        private bool AuthorizeSee(ContentPart part, HiddenStringField field) {
            return _authorizer.Authorize(new HiddenFieldSeePermission(part, field), part);
        }

        protected override void Importing(ContentPart part, HiddenStringField field, ImportContentContext context) {
            var importedText = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "Text");
            if (importedText != null) {
                field.Value = importedText;
            }
        }

        protected override void Exporting(ContentPart part, HiddenStringField field, ExportContentContext context) {
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Text", field.Value);
        }

        protected override void Cloning(ContentPart part, HiddenStringField originalField, HiddenStringField cloneField, CloneContentContext context) {
            var settings = originalField.PartFieldDefinition.Settings.GetModel<HiddenStringFieldSettings>();
            if (!settings.Tokenized) {
                //Token replacement happens OnCreated, so if the field is tokenized we don't clone the Value
                cloneField.Value = originalField.Value;
            }
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Value"), T("The string associated with the field."))
                .Enumerate<HiddenStringField>(() => field => new[] { field.Value });
        }

    }
}