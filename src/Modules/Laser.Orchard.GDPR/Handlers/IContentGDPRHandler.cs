using Orchard;

namespace Laser.Orchard.GDPR.Handlers {
    /// <summary>
    /// Interface for the handlers that will be used to specify the anonymization and erasure
    /// behaviours specific for parts and fields. Generally, rather than implementing this, 
    /// developers should implement the abstract class ContentGDPRHandler. The handlers have
    /// been designed in the same way as the usual ContentHandler from Orchard.ContentManagement,
    /// hence they should be used similarly.
    /// Implementers should take care to perform all operations on all versions of contents.
    /// </summary>
    public interface IContentGDPRHandler : IDependency {
        void Anonymizing(AnonymizeContentContext context);
        void Anonymized(AnonymizeContentContext context);

        void Erasing(EraseContentContext context);
        void Erased(EraseContentContext context);
    }
}
