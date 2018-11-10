using Laser.Orchard.GDPR.Handlers;
using Laser.Orchard.GDPR.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.GDPR.Services {
    public interface IGDPRProcessAllowedProvider : IDependency {

        /// <summary>
        /// This method checks whether the GDPR compliance process represented by the context
        /// should go ahead, or be prevented for some reason.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The method returns true if the processing is allowed, false if it should not
        /// go ahead.</returns>
        bool ProcessIsAllowed(GDPRContentContext context);

        /// <summary>
        /// This method checks whether the any GDPR compliance process may go ahead on the item
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns>The method returns true if the processing is allowed, false if it should not
        /// go ahead.</returns>
        bool ProcessIsAllowed(ContentItem contentItem);

        /// <summary>
        /// This method checks whether the any GDPR compliance process may go ahead on the part's item
        /// </summary>
        /// <param name="part"></param>
        /// <returns>The method returns true if the processing is allowed, false if it should not
        /// go ahead.</returns>
        bool ProcessIsAllowed(GDPRPart part);
    }
}
