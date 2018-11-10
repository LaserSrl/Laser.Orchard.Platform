using Laser.Orchard.ShareLink.Models;
using Orchard;

namespace Laser.Orchard.ShareLink.Servicies {
    public interface IShareLinkPriorityProvider : IDependency {
        /// <summary>
        /// Gets the priority for the ShareLinkPart in the determination of the meta tags for the page.
        /// There is no guarantee on the order that providers will be processed, so having same-priority 
        /// providers may have unpredictable results.
        /// </summary>
        /// <param name="part">The ShareLinkPart to be tested for priority.</param>
        /// <returns>The integer priority for this part based on the current provider.</returns>
        int GetPriority(ShareLinkPart part);
    }
}
