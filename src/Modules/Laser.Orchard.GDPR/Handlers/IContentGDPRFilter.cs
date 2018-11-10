using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.GDPR.Handlers {
    public interface IContentGDPRFilter : IContentFilter {
        void Anonymizing(AnonymizeContentContext context);
        void Anonymized(AnonymizeContentContext context);

        void Erasing(EraseContentContext context);
        void Erased(EraseContentContext context);
    }
}
