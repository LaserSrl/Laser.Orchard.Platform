using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Settings {
    public class QuestionnairesPartEditorEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "QuestionnairePart") yield break;
            var model = definition.Settings.GetModel<QuestionnairesPartSettingVM>();

            //model.AvailableModes = Enum.GetValues(typeof(ShareBarMode))
            //    .Cast<int>()
            //    .Select(i =>
            //        new {
            //            Text = Enum.GetName(typeof(ShareBarMode), i),
            //            Value = i
            //        });

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "QuestionnairePart") yield break;

            var model = new QuestionnairesPartSettingVM();
            updateModel.TryUpdateModel(model, "QuestionnairesPartSettingVM", null, null);

            builder.WithSetting("QuestionnairesPartSettingVM.QuestionsLimitsNumber",
            ((Int32)model.QuestionsLimitsNumber).ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.QuestionsSortedRandomlyNumber",
            ((Int32)model.QuestionsSortedRandomlyNumber).ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.QuestionsResponseLimitsNumber",
            ((Int32)model.QuestionsResponseLimitsNumber).ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.ShowCorrectResponseFlag",
            ((Boolean)model.ShowCorrectResponseFlag).ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.RandomResponse",
            ((Boolean)model.RandomResponse).ToString());

            builder.WithSetting("QuestionnairesPartSettingVM.EnableQuestionImage",
            ((Boolean)model.EnableQuestionImage).ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.EnableAnswerImage",
            ((Boolean)model.EnableAnswerImage).ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.QuestionImageLimitNumber",
            ((Int32)model.QuestionImageLimitNumber).ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.AnswerImageLimitNumber",
            ((Int32)model.AnswerImageLimitNumber).ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.AllowSections", model.AllowSections.ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.AllowConditions", model.AllowConditions.ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.AllowSingleChoice", model.AllowSingleChoice.ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.AllowMultiChoice", model.AllowMultiChoice.ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.AllowOpenAnswers", model.AllowOpenAnswers.ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.QuestionnaireContext", model.QuestionnaireContext);
            builder.WithSetting("QuestionnairesPartSettingVM.ForceAnonymous", model.ForceAnonymous.ToString());
            builder.WithSetting("QuestionnairesPartSettingVM.ShowLatestAnswers", model.ShowLatestAnswers.ToString());

            yield return DefinitionTemplate(model);
        }
    }
}