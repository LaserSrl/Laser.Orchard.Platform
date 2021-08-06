using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.CustomForms.Services;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.StartupConfig.FrontendEditorConfiguration")]
    public class ConfigurableEditorBuilderWrapper : IEditorBuilderWrapper {
        private readonly IContentManager _contentManager;
        private readonly IFrontEndEditService _frontEndEditService;

        public ConfigurableEditorBuilderWrapper(
            IContentManager contentManager,
            IFrontEndEditService frontEndEditService) {

            _contentManager = contentManager;
            _frontEndEditService = frontEndEditService;
        }

        public dynamic BuildEditor(IContent content) {
            if (content.Is<FrontendEditConfigurationPart>()) {
                return _frontEndEditService.BuildFrontEndShape(
                    _contentManager.BuildEditor(content),
                    PartTest,
                    FieldTest);
            }
            return _contentManager.BuildEditor(content);
        }

        public dynamic UpdateEditor(IContent content, IUpdateModel updateModel) {
            if (content.Is<FrontendEditConfigurationPart>()) {
                return _frontEndEditService.BuildFrontEndShape(
                    _contentManager.UpdateEditor(content, updateModel),
                    PartTest,
                    FieldTest);
            }
            return _contentManager.UpdateEditor(content, updateModel);
        }

        Func<ContentTypePartDefinition, string, bool> PartTest =
            (ctpd, typeName) =>
                ctpd.PartDefinition.Name == typeName || // for fields added to the ContentType directly
                AllowEdit(ctpd);
        Func<ContentPartFieldDefinition, bool> FieldTest =
            (cpfd) =>
                AllowEdit(cpfd);

        private static bool AllowEdit(ContentPartFieldDefinition cpfd) {
            var settings = cpfd
                ?.Settings
                ?.GetModel<FrontendEditorConfigurationSettings>();
            if (settings != null) {
                return settings.AllowFrontEndEdit;
            }
            return false;
        }
        private static bool AllowEdit(ContentTypePartDefinition ctpd) {
            var settings = ctpd
                ?.Settings
                ?.GetModel<FrontendEditorConfigurationSettings>();
            if (settings != null) {
                return settings.AllowFrontEndEdit;
            }
            return false;
        }
    }
}