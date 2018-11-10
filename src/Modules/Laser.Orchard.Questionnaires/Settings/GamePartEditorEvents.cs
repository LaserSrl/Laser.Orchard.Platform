using Laser.Orchard.TemplateManagement.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Questionnaires.Settings {

    public class GamePartEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly ITemplateService _templateService;

        public GamePartEditorEvents(ITemplateService templateService) {
            _templateService = templateService;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "GamePart") yield break;
            var model = definition.Settings.GetModel<GamePartSettingVM>();
            model.ListTemplate = _templateService.GetTemplates().Select(s => new TemplateLookup {
                Id = s.Id,
                Name = s.Title
            }).ToList();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "GamePart")
                yield break;

            var model = new GamePartSettingVM();
            if (updateModel.TryUpdateModel(model, "GamePartSettingVM", null, null)) {
                builder.WithSetting("GamePartSettingVM.Ranking",
                    model.Ranking.ToString());
                builder.WithSetting("GamePartSettingVM.SendEmail",
                    model.SendEmail.ToString());
                builder.WithSetting("GamePartSettingVM.template",
                    model.Template.ToString());
                builder.WithSetting("GamePartSettingVM.EmailRecipe",
                    model.EmailRecipe ?? string.Empty);
            }
        }
    }
}