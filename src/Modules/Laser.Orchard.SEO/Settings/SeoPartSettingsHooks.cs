using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace Laser.Orchard.SEO.Settings {
    public class SeoPartSettingsHooks : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition) {

            if (definition.PartDefinition.Name != "SeoPart") yield break;

            var model = definition.Settings.GetModel<SeoPartSettings>();

            model.Templates = getMicrodataTemplate();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name == "SeoPart") {
                var model = new SeoPartSettings();
                updateModel.TryUpdateModel(model, "SeoPartSettings", null, null);

                if(model.Templates == null) {
                    model.Templates = getMicrodataTemplate();
                }

                builder.WithSetting("SeoPartSettings.RobotsNoIndex", model.RobotsNoIndex.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoFollow", model.RobotsNoFollow.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoSnippet", model.RobotsNoSnippet.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoOdp", model.RobotsNoOdp.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoArchive", model.RobotsNoArchive.ToString());
                builder.WithSetting("SeoPartSettings.RobotsUnavailableAfter", model.RobotsUnavailableAfter.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoImageIndex", model.RobotsNoImageIndex.ToString());
                builder.WithSetting("SeoPartSettings.GoogleNoSiteLinkSearchBox", model.GoogleNoSiteLinkSearchBox.ToString());
                builder.WithSetting("SeoPartSettings.GoogleNoTranslate", model.GoogleNoTranslate.ToString());
                if (model.CanonicalUrl != null) {
                    builder.WithSetting("SeoPartSettings.CanonicalUrl", model.CanonicalUrl.ToString());
                }
                else
                    builder.WithSetting("SeoPartSettings.CanonicalUrl","");
                builder.WithSetting("SeoPartSettings.JsonLd", model.JsonLd != null ? model.JsonLd.ToString() : "");

                builder.WithSetting("SeoPartSettings.ShowAggregatedMicrodata", model.ShowAggregatedMicrodata.ToString());

                yield return DefinitionTemplate(model);
            }

            yield break;
        }

        private Dictionary<string, string> getMicrodataTemplate() {
            Dictionary<string, string> templates = new Dictionary<string, string>();

            string[] filelist = Directory.GetFiles(HostingEnvironment.MapPath(@"~/Modules/Laser.Orchard.SEO/MicrodataTemplate"));
            foreach (string filepath in filelist) {
                templates.Add(Path.GetFileNameWithoutExtension(filepath), File.ReadAllText(filepath));
            }
            //da fare il blocco try per IOException
            return templates;
        }

    }
}