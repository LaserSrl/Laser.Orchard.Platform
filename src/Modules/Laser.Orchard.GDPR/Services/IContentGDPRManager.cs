using Laser.Orchard.GDPR.Handlers;
using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.GDPR.Services {
    /// <summary>
    /// Functionality to deal with anonymization and erasure of Orchard's ContentItems,
    /// ContentParts and ContentFields.
    /// </summary>
    public interface IContentGDPRManager : IDependency {

        /// <summary>
        /// Performs erasure of personal identifiable information from all versions of the
        /// ContentItem, as configured in the system.
        /// </summary>
        /// <param name="contentItem">The ContentItem from which personal identifiable information
        /// will be erased.</param>
        /// <remarks>Attempting to process a ContentItem lacking a configuration for GDPR (i.e. that
        /// does not have a GDPRPart) or configured to be protected doesn't throw exception, but
        /// silently does nothing.</remarks>
        void Erase(ContentItem contentItem);

        /// <summary>
        /// Performs erasure of personal identifiable information from all versions of the
        /// ContentItem, as configured in the system.
        /// It also carries information from another context object, such as a list of the items that
        /// are going through the processing.
        /// </summary>
        /// <param name="contentItem">The ContentItem from which personal identifiable information
        /// will be erased.</param>
        /// <param name="previousContext">An existing context object, representing the processing that
        /// has been done so far.</param>
        /// <remarks>This version of the Erase method should be used especially when invoking the
        /// processing of "related" ContentItems to one that is being processed. This is helpful in
        /// avoiding infinite recursion, for example, or keeping track of other items being processed
        /// to perhaps use some information from them.
        /// We use the abstract class fo rthe context, because on principle an anonymization may be 
        /// called while processing an Erasure and vice-versa.</remarks>
        void Erase(ContentItem contentItem, GDPRContentContext previousContext);

        /// <summary>
        /// Performs anonymization of all versions of the ContentItem, as configured in the system.
        /// </summary>
        /// <param name="contentItem">The ContentItem that will be anonymized.</param>
        /// <remarks>Attempting to process a ContentItem lacking a configuration for GDPR (i.e. that
        /// does not have a GDPRPart) or configured to be protected doesn't throw exception, but
        /// silently does nothing.</remarks>
        void Anonymize(ContentItem contentItem);

        /// <summary>
        /// Performs anonymization of all versions of the ContentItem, as configured in the system.
        /// It also carries information from another context object, such as a list of the items that
        /// are going through the processing.
        /// </summary>
        /// <param name="contentItem">The ContentItem that will be anonymized.</param>
        /// <param name="previousContext">An existing context object, representing the processing that
        /// has been done so far.</param>
        /// <remarks>This version of the Anonymize method should be used especially when invoking the
        /// processing of "related" ContentItems to one that is being processed. This is helpful in
        /// avoiding infinite recursion, for example, or keeping track of other items being processed
        /// to perhaps use some information from them.
        /// We use the abstract class fo rthe context, because on principle an anonymization may be 
        /// called while processing an Erasure and vice-versa.</remarks>
        void Anonymize(ContentItem contentItem, GDPRContentContext previousContext);
    }
}
