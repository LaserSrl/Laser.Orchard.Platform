using Laser.Orchard.PrivateMedia.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.MediaLibrary.Models;

namespace Laser.Orchard.PrivateMedia.Handlers {
    public class PrivateMediaPartHandler : ContentHandler {
        public PrivateMediaPartHandler(IRepository<PrivateMediaPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnLoaded<MediaPart>((context, part) => {
                if (part.FolderPath.Contains("Private")) {
                    part.ContentItem.As<PrivateMediaPart>().IsPrivate = true;
                }
                else
                    part.ContentItem.As<PrivateMediaPart>().IsPrivate = false;
            });
        }
    }
}