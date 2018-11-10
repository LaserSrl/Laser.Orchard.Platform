using Orchard;
using Orchard.Environment.Extensions.Models;
using System.Collections.Generic;

namespace Laser.Orchard.GDPR.Features {
    public interface IKeyFeaturesProvider : IDependency {
        /// <summary>
        /// Returns all the KeyFeatures that are currently disabled for this provider
        /// </summary>
        /// <returns></returns>
        IEnumerable<FeatureDescriptor> DisabledKeyFeatures();

        /// <summary>
        /// Returns all the KeyFeatures that are currently disabled for this provider and that
        /// have a dependency on the feature with the given name.
        /// </summary>
        /// <param name="dependencyName">The name of the dependency.</param>
        /// <returns></returns>
        IEnumerable<FeatureDescriptor> DisabledKeyFeatures(string dependencyName);

        /// <summary>
        /// Returns all the KeyFeatures that are currently disabled for this provider and that
        /// have a dependency on the give feature.
        /// </summary>
        /// <param name="dependency">The dependecy.</param>
        /// <returns></returns>
        IEnumerable<FeatureDescriptor> DisabledKeyFeatures(Feature dependency);
    }
}
