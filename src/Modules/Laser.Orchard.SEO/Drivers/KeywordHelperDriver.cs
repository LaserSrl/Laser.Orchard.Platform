using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.IO;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.SEO.Drivers {
    [OrchardFeature("Laser.Orchard.KeywordHelper")]
    public class KeywordHelperDriver : ContentPartCloningDriver<KeywordHelperPart> {

        public KeywordHelperDriver() {

        }

        /// <summary>
        /// GET Editor
        /// </summary>
        protected override DriverResult Editor(KeywordHelperPart part, dynamic shapeHelper) {


            if (KeywordHelperKeyword.langDictionary == null) {
                //Load languages from file
                var assembly = Assembly.GetExecutingAssembly();
                var langResourceName = "Laser.Orchard.SEO.LanguageCodes.txt";
                KeywordHelperKeyword.langDictionary = new Dictionary<string, string>(); //value, text
                using (Stream st = assembly.GetManifestResourceStream(langResourceName)) {
                    using (StreamReader reader = new StreamReader(st)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            string[] parts = line.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries); //text, value
                            KeywordHelperKeyword.langDictionary.Add(parts[1], parts[0]);
                        }
                    }
                }
                
            }
            if (KeywordHelperKeyword.regionDictionary == null) {
                //Load regions from file
                var assembly = Assembly.GetExecutingAssembly();
                var regionResourceName = "Laser.Orchard.SEO.CountryCodes.txt";
                KeywordHelperKeyword.regionDictionary = new Dictionary<string, string>(); //value, text
                using (Stream st = assembly.GetManifestResourceStream(regionResourceName)) {
                    using (StreamReader reader = new StreamReader(st)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            string[] parts = line.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries); //text, value
                            KeywordHelperKeyword.regionDictionary.Add(parts[1], parts[0]);
                        }
                    }
                }
            }

            return ContentShape("Parts_KeywordHelper_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/KeywordHelper",
                    Model: new KeywordHelperPartViewModel(part),
                    Prefix: Prefix));
        }

        /// <summary>
        /// POST editor
        /// </summary>
        protected override DriverResult Editor(KeywordHelperPart part, IUpdateModel updater, dynamic shapeHelper) {
            var vm = new KeywordHelperPartViewModel();
            if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                part.Keywords = vm.ListToString();
            }
            return Editor(part,shapeHelper);
        }

        protected override void Cloning(KeywordHelperPart originalPart, KeywordHelperPart clonePart, CloneContentContext context) {
            clonePart.Keywords = originalPart.Keywords;
        }

        protected override void Importing(KeywordHelperPart part, ImportContentContext context) {
            var importedKeywords = context.Attribute(part.PartDefinition.Name, "Keywords");
            if (importedKeywords != null) {
                part.Keywords = importedKeywords;
            }
        }

        protected override void Exporting(KeywordHelperPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Keywords", part.Keywords);
        }



    }
}