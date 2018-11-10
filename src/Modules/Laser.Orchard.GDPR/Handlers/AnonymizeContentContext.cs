using Orchard.ContentManagement;

namespace Laser.Orchard.GDPR.Handlers {
    public class AnonymizeContentContext : GDPRContentContext {

        public AnonymizeContentContext(ContentItem contentItem) : base(contentItem) {
            Erase = false;
        }

        public AnonymizeContentContext(ContentItem contentItem, GDPRContentContext previousContext) 
            : base(contentItem, previousContext) {
            Erase = false;
        }
    }
}