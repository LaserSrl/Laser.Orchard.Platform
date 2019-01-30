using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using System.Web.Script.Serialization;


namespace Laser.Orchard.UserReactions.Settings {
    public class UserReactionsPartSettingsHooks : ContentDefinitionEditorEventsBase 
    {
        private readonly IUserReactionsService _userReactionService;

        public UserReactionsPartSettingsHooks(IUserReactionsService userReactionService) {
            _userReactionService = userReactionService;
        }


        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) 
        {
            if (definition.PartDefinition.Name != "UserReactionsPart") yield break;
            
            var model = definition.Settings.GetModel<UserReactionsPartSettings>();
            
            List<UserReactionsSettingTypesSel> SettingType = new List<UserReactionsSettingTypesSel>();
            
            if(definition.Settings.Count>0)
            {
                SettingType = new JavaScriptSerializer().Deserialize<List<UserReactionsSettingTypesSel>>(definition.Settings["UserReactionsPartSettings.TypeReactionsPartsSelected"]);
            }

            model.TypeReactionsPartsSelected = SettingType;   
            model = _userReactionService.GetSettingPart(model);

            yield return DefinitionTemplate(model);
        }



        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) 
        {            
            if (builder.Name != "UserReactionsPart") yield break;
            var model = new UserReactionsPartSettings();


            updateModel.TryUpdateModel(model, "UserReactionsPartSettings", null, null);
            if(model.TypeReactionsPartsSelected != null){
                string jsonTypeReactions = new JavaScriptSerializer().Serialize(model.TypeReactionsPartsSelected.ToArray());
                builder.WithSetting("UserReactionsPartSettings.Filtering", ((bool)model.Filtering).ToString());
                builder.WithSetting("UserReactionsPartSettings.TypeReactionsPartsSelected", jsonTypeReactions);
                builder.WithSetting("UserReactionsPartSettings.UserChoiceBehaviour", ((UserChoiceBehaviourValues)model.UserChoiceBehaviour).ToString());
            }
    
            yield return DefinitionTemplate(model);
        }
    }
}