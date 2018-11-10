using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Laser.Orchard.StartupConfig.Services {

    public interface IContentExtensionsServices : IDependency {

        IEnumerable<ParentContent> ContentPickerParents(int contentId, string[] contentTypes);
        [Obsolete("Replaced by Laser.Orchard.ContentExtension.IContentExtensionService")]
        Response StoreInspectExpando(ExpandoObject theExpando, ContentItem TheContentItem);
        [Obsolete("Replaced by Laser.Orchard.StartupConfig.IUtilsServices")]
        void StoreInspectExpandoFields(List<ContentPart> listpart, string key, object value, ContentItem theContentItem);
        [Obsolete("Replaced by Laser.Orchard.ContentExtension.IContentExtensionService")]
        bool FileAllowed(string filename);
    }
}