using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Settings {
    public class UserReactionsSettingsHooks : ContentDefinitionEditorEventsBase {
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public UserReactionsSettingsHooks(INotifier notifier) {
            _notifier = notifier;
        }
        
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name == "UserReactionsPart") {
                _notifier.Warning(T("Please verify authorization for UserReactions on this Content Type."));
            }
            return base.TypePartEditorUpdate(builder, updateModel);
        }
    }
}