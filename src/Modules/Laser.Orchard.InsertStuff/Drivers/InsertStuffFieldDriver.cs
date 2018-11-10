using Laser.Orchard.InsertStuff.Fields;
using Laser.Orchard.InsertStuff.Settings;
using Laser.Orchard.InsertStuff.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Laser.Orchard.InsertStuff.Drivers {

    public class InsertStuffFieldDriver : ContentFieldDriver<InsertStuffField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Laser.Orchard.InsertStuff";

        public InsertStuffFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(InsertStuffField field, ContentPart part) {
            return field.Name;
        }

        /// <summary>
        /// Gets called when the field is displayed
        /// </summary>
        protected override DriverResult Display(ContentPart part, InsertStuffField field, string displayType, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<InsertStuffFieldSettings>();

            var viewModel = new InsertStuffViewModel {
                DisplayName = field.Name
            };

            return ContentShape("Fields_Laser_Orchard_InsertStuff", GetDifferentiator(field, part),
                () => shapeHelper.Fields_Laser_Orchard_InsertStuff(ContentPart: part, ContentField: field, Model: viewModel));
        }

        /// <summary>
        /// Gets called when the field is used in edit mode (GET)
        /// </summary>
        protected override DriverResult Editor(ContentPart part, Laser.Orchard.InsertStuff.Fields.InsertStuffField field, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<InsertStuffFieldSettings>();

            var viewModel = new InsertStuffViewModel {
                DisplayName = field.DisplayName,
                StyleList = string.IsNullOrWhiteSpace(settings.StyleList) ? new List<string>() : new List<string>(settings.StyleList.Split('\n')),
                ScriptList = string.IsNullOrWhiteSpace(settings.ScriptList) ? new List<string>() : new List<string>(settings.ScriptList.Split('\n')),
                RawHtml = settings.RawHtml,
                OnFooter = settings.OnFooter
            };

            return ContentShape("Fields_Laser_Orchard_InsertStuff_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part)));
        }

        /// <summary>
        /// Gets called when the field is used in edit mode (POST)
        /// </summary>
        protected override DriverResult Editor(ContentPart part, Laser.Orchard.InsertStuff.Fields.InsertStuffField field, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, field, shapeHelper);
        }

        protected override void Describe(DescribeMembersContext context) {
            context.Member(null, typeof(string), T("Value"), T("The selected values of the field."));
        }
    }
}