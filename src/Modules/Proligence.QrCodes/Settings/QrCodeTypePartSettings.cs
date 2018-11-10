namespace Proligence.QrCodes.Settings
{
    using System.Collections.Generic;

    using Orchard.ContentManagement;
    using Orchard.ContentManagement.MetaData;
    using Orchard.ContentManagement.MetaData.Builders;
    using Orchard.ContentManagement.MetaData.Models;
    using Orchard.ContentManagement.ViewModels;

    public class QrCodeTypePartSettings
    {
        private string _value;

        public string Value
        {
            get
            {
                return string.IsNullOrWhiteSpace(_value) ? "{Content.DisplayText}" : _value;
            }
            set
            {
                _value = value;
            }
        }

        private int _size;

        public int Size
        {
            get
            {
                return _size == default(int) ? 100 : _size;
            }
            set
            {
                _size = value;
            }
        }
    }

    /// <summary>
    /// Overrides default editors to enable putting settings on Twitter Connect part.
    /// </summary>
    public class ShareBarSettingsHooks : ContentDefinitionEditorEventsBase
    {
        /// <summary>
        /// Overrides editor shown when part is attached to content type. Enables adding setting field to the content part
        /// attached.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition)
        {
            if (definition.PartDefinition.Name != "QrCodePart")
                yield break;
            var model = definition.Settings.GetModel<QrCodeTypePartSettings>();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            if (builder.Name != "QrCodePart")
                yield break;

            var model = new QrCodeTypePartSettings();
            updateModel.TryUpdateModel(model, "QrCodeTypePartSettings", null, null);
            builder.WithSetting("QrCodeTypePartSettings.Value", model.Value);
            builder.WithSetting("QrCodeTypePartSettings.Size", model.Size.ToString());
            yield return DefinitionTemplate(model);
        }

    }
}
