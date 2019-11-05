using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using System.Web.Hosting;
using System;
using Laser.Orchard.Commons.Helpers;

namespace Laser.Orchard.ExternalContent.Settings {
    public class FieldExternalEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly ShellSettings _shellSettings;
        public FieldExternalEditorEvents(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "FieldExternal") {
                var model = definition.Settings.GetModel<FieldExternalSetting>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "FieldExternal") {
                yield break;
            }
            var model = new FieldExternalSetting();
            if (updateModel.TryUpdateModel(model, "FieldExternalSetting", null, null)) {
                builder.WithSetting("FieldExternalSetting.Required", model.Required.ToString());
                builder.WithSetting("FieldExternalSetting.CacheMinute", model.CacheMinute.ToString());
                builder.WithSetting("FieldExternalSetting.ExternalURL", model.ExternalURL);
                builder.WithSetting("FieldExternalSetting.NoFollow", model.NoFollow.ToString());
                builder.WithSetting("FieldExternalSetting.GenerateL", model.GenerateL.ToString());
                builder.WithSetting("FieldExternalSetting.HttpVerb", model.HttpVerb.ToString());
                builder.WithSetting("FieldExternalSetting.HttpDataType", model.HttpDataType.ToString());
                builder.WithSetting("FieldExternalSetting.BodyRequest", model.BodyRequest);
                builder.WithSetting("FieldExternalSetting.CertificateRequired", model.CertificateRequired.ToString());
                builder.WithSetting("FieldExternalSetting.CerticateFileName", model.CerticateFileName);
                builder.WithSetting("FieldExternalSetting.CacheInput", model.CacheInput);
                builder.WithSetting("FieldExternalSetting.CacheToFileSystem", model.CacheToFileSystem.ToString());
                builder.WithSetting("FieldExternalSetting.ScheduledMinute", model.ScheduledMinute.ToString());
                builder.WithSetting("FieldExternalSetting.DataType", model.DataType.ToString());
                builder.WithSetting("FieldExternalSetting.AdditionalHeadersText", model.AdditionalHeadersText);
                builder.WithSetting("FieldExternalSetting.InternalHostNameForScheduledTask", model.InternalHostNameForScheduledTask);

                if (model.CertificatePrivateKey == "(none)") { // empty private key
                    builder.WithSetting("FieldExternalSetting.CertificatePrivateKey", "");
                } else if (!String.IsNullOrEmpty(model.CertificatePrivateKey)) { //save new key
                    builder.WithSetting("FieldExternalSetting.CertificatePrivateKey", model.CertificatePrivateKey.EncryptString(_shellSettings.EncryptionKey));
                } // otherwise keep private key untouched

                if (model.CertificateRequired) {
                    string mobile_folder = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSettings.Name + @"\ExternalFields\";
                    if (!System.IO.Directory.Exists(mobile_folder))
                        System.IO.Directory.CreateDirectory(mobile_folder);

                }
            }
            yield return DefinitionTemplate(model);
        }
    }
}