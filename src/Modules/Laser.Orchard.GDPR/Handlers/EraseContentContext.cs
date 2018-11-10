using Orchard.ContentManagement;

namespace Laser.Orchard.GDPR.Handlers {
    public class EraseContentContext : GDPRContentContext{

        public EraseContentContext(ContentItem contentItem) : base(contentItem) {
            Erase = true;
        }

        public EraseContentContext(ContentItem contentItem, GDPRContentContext previousContext) 
            : base(contentItem, previousContext) {
            Erase = true;
        }
    }
}