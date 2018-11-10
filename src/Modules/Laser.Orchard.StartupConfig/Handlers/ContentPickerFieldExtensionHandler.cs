using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.Settings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class ContentPickerFieldExtensionHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        public ContentPickerFieldExtensionHandler(IContentManager contentManager, IControllerContextAccessor controllerContextAccessor,
                                                  ILocalizationService localizationService, IOrchardServices orchardServices, INotifier notifier) {
            _contentManager = contentManager;
            _controllerContextAccessor = controllerContextAccessor;
            _localizationService = localizationService;
            _notifier = notifier;
            _orchardServices = orchardServices;

            OnPublishing<CommonPart>((context, part) => CascadePublish(context.ContentItem));

            T = NullLocalizer.Instance;
        }

        private void CascadePublish(ContentItem contentItem) {
            var fields = contentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(ContentPickerField).Name)).Cast<ContentPickerField>();
            foreach (ContentPickerField field in fields) {
                var settings = field.PartFieldDefinition.Settings.GetModel<ContentPickerFieldExtensionSettings>();
                if (settings.CascadePublish) {
                    foreach (Int32 id in field.Ids) {
                        ContentItem contentlinked = _orchardServices.ContentManager.Get(id, VersionOptions.Published);
                        if (contentlinked == null) {
                            contentlinked = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
                            _orchardServices.ContentManager.Publish(contentlinked);
                            _notifier.Add(NotifyType.Information, (T("Linked {0} has been published", contentlinked.ContentType)));
                        }
                    }
                }
            }
        }
    }
}