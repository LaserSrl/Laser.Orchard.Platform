using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Linq;

namespace Laser.Orchard.Sharing.Settings {

    public class ShareBarTypePartSettings {
        public ShareBarTypePartSettings() {
            Buttons = "";
            IconSize = 32;
        }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Media { get; set; }
        public string Description { get; set; }
        public string Buttons { get; set; }
        public int IconSize { get; set; }
    }

    /// <summary>
    /// Overrides default editors to enable putting settings on Twitter Connect part.
    /// </summary>
    public class ShareBarSettingsHooks : ContentDefinitionEditorEventsBase {

        /// <summary>
        /// Overrides editor shown when part is attached to content type. Enables adding setting field to the content part
        /// attached.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ShareBarPart")
                yield break;
            var model = definition.Settings.GetModel<ShareBarTypePartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ShareBarPart")
                yield break;

            var model = new ShareBarTypePartSettings();
            updateModel.TryUpdateModel(model, "ShareBarTypePartSettings", null, null);
            builder.WithSetting("ShareBarTypePartSettings.Url",
                model.Url);
            builder.WithSetting("ShareBarTypePartSettings.Title",
                model.Title);
            builder.WithSetting("ShareBarTypePartSettings.Media",
                model.Media);
            builder.WithSetting("ShareBarTypePartSettings.Description",
                model.Description);
            builder.WithSetting("ShareBarTypePartSettings.Buttons",
                model.Buttons);
            builder.WithSetting("ShareBarTypePartSettings.IconSize",
                model.IconSize.ToString());
            yield return DefinitionTemplate(model);
        }

    }
}
